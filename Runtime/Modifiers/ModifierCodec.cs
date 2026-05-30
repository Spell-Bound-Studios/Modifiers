// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using Spellbound.Core.Logging;
using Spellbound.Core.Packing;
using UnityEngine.Scripting;

namespace Spellbound.Modifiers {
    /// <summary>
    /// <b>Why this exists:</b> inventory item data, save sections, and network frames each need
    /// to round-trip a <c>List&lt;SbModifier&gt;</c> where individual entries are different
    /// concrete subclasses (<c>StatAffix</c>, <see cref="TraitRef"/>, future game-specific Affix
    /// types). Without this codec, every consumer would hand-roll a switch on type name to encode
    /// and decode their lists. With it, consumers call <see cref="Encode"/> / <see cref="Decode"/>
    /// and get a byte[] payload that round-trips faithfully.
    /// </summary>
    /// <remarks>
    /// <para><b>Wire format per entry:</b></para>
    /// <list type="bullet">
    /// <item><c>[Tag.TraitRef][TraitRef.Pack-bytes]</c> — 1-byte tag, then the TraitRef's
    /// hashed-id payload (4 bytes). One class, no type-name needed; the tag IS the type.</item>
    /// <item><c>[Tag.Affix][typeName length-prefixed string][Affix.Pack-bytes]</c> — 1-byte tag,
    /// then the concrete subclass's full type name, then its packed state. The type name lets
    /// consumer-side Affix subclasses ride the same tag — the codec doesn't need to know them at
    /// compile time; reflection instantiates on decode.</item>
    /// </list>
    /// <para><b>Tag stability is load-bearing:</b> the byte values in <see cref="Tag"/> are THE
    /// wire identity for each subclass family. <b>Never renumber existing tags</b> or every saved
    /// item / save file / queued network frame breaks. Adding a new modifier shape is additive: a
    /// new tag constant + a new case in <see cref="ReadTagged"/> + a new branch in
    /// <see cref="ResolveTag"/>.</para>
    /// <para><b>IL2CPP stripping caveat:</b> the codec instantiates concrete <see cref="Affix"/>
    /// subclasses by reflection (<see cref="Activator.CreateInstance(Type)"/>), so on IL2CPP
    /// builds the linker may strip subclasses that have no compile-time reference. Consumer-side
    /// Affix subclasses must be <c>[Preserve]</c>-marked (or covered by a <c>link.xml</c> entry)
    /// to survive build-time stripping. Lib-side types ride the codec's existing
    /// <c>[Preserve]</c>.</para>
    /// <para><b>Known concerns / scheduled rework:</b> see
    /// <c>_GameLogic/Docs/ModifierCodecRework.md</c> for the open architectural questions —
    /// type-name string bloat, repeated reflection lookups, no format-version byte, and whether
    /// the polymorphic-list-codec pattern should be a Core primitive instead of lib-local.</para>
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
            catch (Exception ex) {
                Log.Error($"Malformed payload; returning empty list. ({ex.Message})");

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
                throw new InvalidOperationException($"Unknown tag {tag}");
            }

            if (mod == null)
                throw new InvalidOperationException($"Failed to instantiate for tag {tag}");

            mod.Unpack(ref buffer);

            return mod;
        }

        private static byte ResolveTag(SbModifier mod) {
            return mod switch {
                TraitRef _ => Tag.TraitRef,
                Affix _ => Tag.Affix,
                _ => throw new InvalidOperationException($"Unsupported modifier type: {mod.GetType().Name}")
            };
        }

        // Cache populated lazily on first lookup. Negative results (type-not-found) also cached
        // so repeated decodes of corrupt data don't re-walk every assembly each time.
        private static readonly Dictionary<string, Type> _typeCache = new();

        private static SbModifier InstantiateByTypeName(string fullName) {
            if (_typeCache.TryGetValue(fullName, out var cached))
                return cached != null ? (SbModifier)Activator.CreateInstance(cached) : null;

            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies()) {
                var type = asm.GetType(fullName);

                if (type == null)
                    continue;

                _typeCache[fullName] = type;

                return (SbModifier)Activator.CreateInstance(type);
            }

            _typeCache[fullName] = null;
            Log.Error($"Type '{fullName}' not found in any loaded assembly.");

            return null;
        }
    }
}
