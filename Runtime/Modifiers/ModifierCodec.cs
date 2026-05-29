// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using Spellbound.Core.Logging;
using Spellbound.Core.Packing;
using UnityEngine.Scripting;

namespace Spellbound.Modifiers {
    /// <summary>
    /// Polymorphic codec for <see cref="SbModifier"/> lists in byte[]-shaped storage (inventory
    /// item data, save sections, network frames). Each entry is encoded with a 1-byte type tag
    /// followed by that subclass's <c>Pack</c> bytes; decode flips the tag back into an
    /// instantiated subclass and calls <c>Unpack</c>.
    /// </summary>
    /// <remarks>
    /// <para>Type tags are <see cref="byte"/>-sized (0–255) and centralized in
    /// <see cref="Tag"/>. Adding a new modifier shape to the codec is one new tag + one new case
    /// in <see cref="ReadTagged"/>. The tag is THE wire identity for the subclass — never
    /// renumber existing tags or saved data breaks.</para>
    /// <para><b>Affix entries</b> carry their concrete subclass's full type name as part of the
    /// encoded payload (<c>[Tag.Affix][type-name-string][Pack-bytes]</c>), so any concrete
    /// <see cref="Affix"/> subclass shipped by a consumer can ride the same tag without the codec
    /// hard-knowing each one. The named type is instantiated via reflection on decode.
    /// <b>TraitRef entries</b> don't carry a type name — there is only one TraitRef class.</para>
    /// </remarks>
    public static class ModifierCodec {
        /// <summary>
        /// Type tags for polymorphic encoding. Stable wire identifiers; never renumber.
        /// </summary>
        public static class Tag {
            public const byte Affix = 0;
            public const byte TraitRef = 1;
        }

        /// <summary>
        /// Encode a list of polymorphic <see cref="SbModifier"/> instances into a byte[]. Each
        /// entry gets a type tag + that subclass's packed bytes.
        /// </summary>
        public static byte[] Encode(IReadOnlyList<SbModifier> modifiers) {
            if (modifiers == null || modifiers.Count == 0)
                return Array.Empty<byte>();

            return Packer.BuildPayload((ref Span<byte> buffer) => {
                Packer.WriteInt(ref buffer, modifiers.Count);

                foreach (var mod in modifiers) {
                    if (mod == null)
                        continue;

                    WriteTagged(ref buffer, mod);
                }
            });
        }

        /// <summary>
        /// Decode a byte[] back into a list of <see cref="SbModifier"/> instances. Returns an
        /// empty list on malformed data — caller treats that as "no modifiers."
        /// </summary>
        public static List<SbModifier> Decode(byte[] data) {
            if (data == null || data.Length == 0)
                return new List<SbModifier>();

            try {
                ReadOnlySpan<byte> span = data;
                var count = Packer.ReadInt(ref span);
                var result = new List<SbModifier>(count);

                for (var i = 0; i < count; i++) {
                    var mod = ReadTagged(ref span);

                    if (mod != null)
                        result.Add(mod);
                }

                return result;
            }
            catch {
                Log.Warn("[ModifierCodec] Malformed payload; returning empty list.");

                return new List<SbModifier>();
            }
        }

        private static void WriteTagged(ref Span<byte> buffer, SbModifier mod) {
            var tag = ResolveTag(mod);
            Packer.WriteByte(ref buffer, tag);

            // Affix carries its concrete type name so any consumer-side Affix subclass can be
            // instantiated at decode time without the codec hard-knowing it. TraitRef does not —
            // there's exactly one TraitRef class, no polymorphism on that tag.
            if (tag == Tag.Affix)
                Packer.WriteString(ref buffer, mod.GetType().FullName);

            mod.Pack(ref buffer);
        }

        [Preserve]
        private static SbModifier ReadTagged(ref ReadOnlySpan<byte> buffer) {
            var tag = Packer.ReadByte(ref buffer);

            SbModifier mod;

            if (tag == Tag.TraitRef) {
                mod = new TraitRef();
            }
            else if (tag == Tag.Affix) {
                var typeName = Packer.ReadString(ref buffer);
                mod = InstantiateByTypeName(typeName);
            }
            else {
                throw new InvalidOperationException($"[ModifierCodec] Unknown tag {tag}");
            }

            if (mod == null)
                throw new InvalidOperationException($"[ModifierCodec] Failed to instantiate for tag {tag}");

            mod.Unpack(ref buffer);

            return mod;
        }

        private static byte ResolveTag(SbModifier mod) {
            return mod switch {
                TraitRef _ => Tag.TraitRef,
                Affix _ => Tag.Affix,
                _ => throw new InvalidOperationException($"[ModifierCodec] Unsupported modifier type: {mod.GetType().Name}")
            };
        }

        private static SbModifier InstantiateByTypeName(string fullName) {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies()) {
                var type = asm.GetType(fullName);

                if (type != null)
                    return (SbModifier)Activator.CreateInstance(type);
            }

            Log.Error($"[ModifierCodec] Type '{fullName}' not found in any loaded assembly.");

            return null;
        }
    }
}
