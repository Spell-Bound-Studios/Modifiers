// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using Spellbound.Core.Logging;
using Spellbound.Core.Packing;

namespace Spellbound.Modifiers {
    /// <summary>
    /// Round-trips a polymorphic <c>List&lt;SbModifier&gt;</c> — entries of different concrete types
    /// (<c>StatAffix</c>, <see cref="TraitRef"/>, …) — to and from a byte[] for inventory item data, save
    /// sections, and network frames. Each entry leads with its <see cref="SmartPackerRegistry"/> hash and is
    /// reconstructed through the registry, so the wire identity is the type's stable <c>[PackerId]</c> rather
    /// than its class name (rename-safe) and decode is a hash lookup rather than reflection. Only modifiers
    /// that carry registry identity (<see cref="ISmartPacker"/> — Affix, TraitRef) are serialized; any other
    /// entry is skipped.
    /// </summary>
    public static class ModifierCodec {
        /// <summary>
        /// Encode the <see cref="ISmartPacker"/> entries of <paramref name="modifiers"/> into a byte[]: a count
        /// followed by each entry's 4-byte type hash and packed state. Returns an empty array for null/empty.
        /// </summary>
        public static byte[] Encode(IReadOnlyList<SbModifier> modifiers) {
            if (modifiers == null || modifiers.Count == 0)
                return Array.Empty<byte>();

            return Packer.BuildPayload((ref Span<byte> buffer) => {
                Packer.WriteInt(ref buffer, CountSerializable(modifiers));

                foreach (var mod in modifiers) {
                    if (mod is not ISmartPacker packer)
                        continue;

                    Packer.WriteUInt(ref buffer, packer.Hash);
                    mod.Pack(ref buffer);
                }
            });
        }

        /// <summary>
        /// Decode a byte[] produced by <see cref="Encode"/> back into a list of modifiers, resolving each
        /// entry's concrete type through the registry. Returns an empty list on malformed data or an
        /// unregistered hash — an entry's byte length is only known once its type is resolved, so a miss
        /// aborts the whole decode rather than guessing.
        /// </summary>
        public static List<SbModifier> Decode(byte[] data) {
            if (data == null || data.Length == 0)
                return new List<SbModifier>();

            try {
                ReadOnlySpan<byte> span = data;
                var count = Packer.ReadInt(ref span);
                var result = new List<SbModifier>(count);

                for (var i = 0; i < count; i++) {
                    var hash = Packer.ReadUInt(ref span);

                    if (!SmartPackerRegistry.TryCreateInstance(hash, out var instance)
                        || instance is not SbModifier mod) {
                        Log.Error($"Hash {hash} did not resolve to a registered SbModifier; aborting decode.");

                        return new List<SbModifier>();
                    }

                    mod.Unpack(ref span);
                    result.Add(mod);
                }

                return result;
            }
            catch (Exception ex) {
                Log.Error($"Malformed payload; returning empty list. ({ex.Message})");

                return new List<SbModifier>();
            }
        }

        private static int CountSerializable(IReadOnlyList<SbModifier> modifiers) {
            var count = 0;

            foreach (var mod in modifiers) {
                if (mod is ISmartPacker)
                    count++;
            }

            return count;
        }
    }
}
