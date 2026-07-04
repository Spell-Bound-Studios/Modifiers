// Copyright 2026 Spellbound Studio Inc.

using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEngine.TestTools.Constraints;
using Is = UnityEngine.TestTools.Constraints.Is;

namespace Spellbound.Modifiers.Tests.Performance {
    public class StatChainPerformanceTests {
        private const int SkillCount = 40;

        private static readonly StatId Fire = new(1u);
        private static readonly StatId Strength = new(2u);

        private Modifiable _player;
        private Modifiable[] _skills;

        [SetUp]
        public void SetUp() {
            _player = new Modifiable();
            _player.Stats.SetBase(Strength, 50f);
            _player.Stats.AddModifier(Fire, ModifierType.Flat, 5f);
            _player.Stats.AddModifier(Fire, ModifierType.Increased, 0.3f);

            _skills = new Modifiable[SkillCount];

            for (var i = 0; i < SkillCount; i++) {
                var skill = new Modifiable { Parent = _player };
                skill.Stats.SetBase(Fire, 30f);
                skill.Stats.AddModifier(Fire, ModifierType.Increased, 0.2f);
                skill.Stats.AddModifier(Fire, ModifierType.More, 0.25f);
                _skills[i] = skill;
            }

            ReadAll();
        }

        private float ReadAll() {
            var total = 0f;

            for (var i = 0; i < SkillCount; i++)
                total += _skills[i].GetValue(Fire);

            return total;
        }

        [Test, Performance]
        public void Read_FortySkills_HotPath() {
            Measure.Method(() => ReadAll())
                    .WarmupCount(10)
                    .MeasurementCount(20)
                    .IterationsPerMeasurement(100)
                    .GC()
                    .Run();
        }

        [Test, Performance]
        public void Read_FortySkills_AfterParentMutation() {
            Measure.Method(() => {
                        _player.Stats.AddModifier(Fire, ModifierType.Flat, 1f, 99u);
                        _player.RemoveSource(99u);
                        ReadAll();
                    })
                    .WarmupCount(10)
                    .MeasurementCount(20)
                    .IterationsPerMeasurement(100)
                    .GC()
                    .Run();
        }

        [Test, Performance]
        public void EquipUnequip_OnParent_CostIsBlockLocal() {
            Measure.Method(() => {
                        _player.Stats.AddModifier(Fire, ModifierType.Flat, 5f, 77u);
                        _player.Stats.AddModifier(Strength, ModifierType.Flat, 5f, 77u);
                        _player.RemoveSource(77u);
                    })
                    .WarmupCount(10)
                    .MeasurementCount(20)
                    .IterationsPerMeasurement(500)
                    .GC()
                    .Run();
        }

        [Test, Performance]
        public void Read_FortySkills_FiveConditionalsEach() {
            for (var i = 0; i < SkillCount; i++) {
                for (var c = 0; c < 5; c++) {
                    _skills[i].Stats.AddModifier(
                        Fire, ModifierType.Increased, 0.1f, (uint)(1000 + c),
                        new StatAtLeast(Strength, 25f));
                }
            }

            ReadAll();

            Measure.Method(() => ReadAll())
                    .WarmupCount(10)
                    .MeasurementCount(20)
                    .IterationsPerMeasurement(100)
                    .GC()
                    .Run();
        }

        [Test]
        public void Read_HotPath_DoesNotAllocate() {
            var skill = _skills[0];
            skill.GetValue(Fire);

            Assert.That(() => { skill.GetValue(Fire); }, Is.Not.AllocatingGCMemory());
        }
    }
}
