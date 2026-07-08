// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Spellbound.Modifiers.Tests {
    public class StatDataTests {
        private static StatId Armor => new(StatRegistry.GetHash("sample_armor"));

        private static RolledModifier RollThickHide(uint sourceId, int seed = 7) =>
                ModifierRegistry.GetDefinition("sample_thick_hide").Roll(new Random(seed), sourceId);

        [Test]
        public void PackedSize_Empty_MatchesWrittenBytes() {
            var data = new StatData();
            var bytes = new byte[64];
            Span<byte> buffer = bytes;
            data.Pack(ref buffer);

            Assert.AreEqual(data.PackedSize, bytes.Length - buffer.Length);
        }

        [Test]
        public void PackUnpack_RoundTrips_AndPackedSizeMatches() {
            var rolled = RollThickHide(11u);

            var data = new StatData {
                resourceData = new List<ResourceData> {
                    new(StatRegistry.GetHash("sample_health"), 100f, 0f, 62.5f)
                },
                modifiers = new List<RolledModifier> { rolled },
                buffs = new List<TimedModifier> {
                    new() { modifier = RollThickHide(22u, 8), duration = 5f, remaining = 2.5f }
                },
                debuffs = new List<TimedModifier> {
                    new() {
                        modifier = new RolledModifier
                                { modifierHash = 9u, sourceId = 3u, baked = Array.Empty<BakedRoll>() },
                        duration = 3f, remaining = 1f
                    }
                }
            };

            var bytes = new byte[512];
            Span<byte> buffer = bytes;
            data.Pack(ref buffer);
            var written = bytes.Length - buffer.Length;

            Assert.AreEqual(data.PackedSize, written);

            var read = new ReadOnlySpan<byte>(bytes, 0, written);
            var copy = new StatData();
            copy.Unpack(ref read);

            Assert.AreEqual(1, copy.ResourceCount);
            Assert.AreEqual(62.5f, copy.resourceData[0].current);
            Assert.AreEqual(1, copy.ModifierCount);
            Assert.AreEqual(rolled.modifierHash, copy.modifiers[0].modifierHash);
            Assert.AreEqual(rolled.sourceId, copy.modifiers[0].sourceId);
            Assert.AreEqual(rolled.baked.Length, copy.modifiers[0].baked.Length);
            Assert.AreEqual(1, copy.BuffCount);
            Assert.AreEqual(2.5f, copy.buffs[0].remaining);
            Assert.AreEqual(1, copy.DebuffCount);
            Assert.AreEqual(1f, copy.debuffs[0].remaining);
        }

        [Test]
        public void ApplyTo_AppliesModifiers() {
            var rolled = RollThickHide(11u);
            var data = new StatData { modifiers = new List<RolledModifier> { rolled } };

            var target = new Modifiable();
            target.Stats.SetBase(Armor, 100f);
            data.ApplyTo(target);

            Assert.AreEqual(100f + rolled.baked[0].value, target.GetValue(Armor), 0.001f);
        }

        [Test]
        public void ApplyTo_RestoresTimedWithRemaining() {
            var data = new StatData {
                buffs = new List<TimedModifier> {
                    new() { modifier = RollThickHide(11u), duration = 5f, remaining = 2f }
                }
            };

            var target = new Modifiable();
            target.Stats.SetBase(Armor, 100f);
            var buffSet = new TimedModifierSet(target);
            data.ApplyTo(target, buffSet);

            Assert.AreEqual(1, buffSet.Active.Count);
            Assert.Greater(target.GetValue(Armor), 100f);

            buffSet.Tick(1.9f);

            Assert.AreEqual(1, buffSet.Active.Count);

            buffSet.Tick(0.2f);

            Assert.AreEqual(0, buffSet.Active.Count);
            Assert.AreEqual(100f, target.GetValue(Armor), 0.001f);
        }

        [Test]
        public void Capture_SnapshotsLiveState() {
            var target = new Modifiable();
            var buffSet = new TimedModifierSet(target);
            buffSet.Apply(RollThickHide(11u), 5f);

            var applied = new List<RolledModifier> { RollThickHide(22u, 8) };
            var data = StatData.Capture(null, applied, buffSet);

            buffSet.Tick(6f);
            applied.Add(RollThickHide(33u, 9));

            Assert.AreEqual(1, data.BuffCount);
            Assert.AreEqual(1, data.ModifierCount);
        }

        [Test]
        public void FullRoundTrip_CaptureToApply() {
            var source = new Modifiable();
            source.Stats.SetBase(Armor, 100f);
            var sourceBuffs = new TimedModifierSet(source);

            var rolled = RollThickHide(11u);
            rolled.TryApplyTo(source);
            sourceBuffs.Apply(RollThickHide(22u, 8), 5f);
            sourceBuffs.Tick(3f);

            var data = StatData.Capture(null, new List<RolledModifier> { rolled }, sourceBuffs);

            var bytes = new byte[512];
            Span<byte> buffer = bytes;
            data.Pack(ref buffer);
            var read = new ReadOnlySpan<byte>(bytes, 0, bytes.Length - buffer.Length);
            var hydrated = new StatData();
            hydrated.Unpack(ref read);

            var target = new Modifiable();
            target.Stats.SetBase(Armor, 100f);
            var targetBuffs = new TimedModifierSet(target);
            hydrated.ApplyTo(target, targetBuffs);

            Assert.AreEqual(source.GetValue(Armor), target.GetValue(Armor), 0.001f);
            Assert.AreEqual(1, targetBuffs.Active.Count);
            Assert.AreEqual(2f, targetBuffs.Active[0].remaining, 0.001f);
        }
    }
}