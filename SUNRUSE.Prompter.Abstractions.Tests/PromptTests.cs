﻿// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using Newtonsoft.Json;
using System;
using System.Collections.Immutable;
using Xunit;

namespace SUNRUSE.Prompter.Abstractions.Tests
{
    public sealed class PromptTests
    {
        [Fact, Trait("Type", "Unit")]
        public void PromptIdSet()
        {
            var promptId = Guid.NewGuid();

            var prompt = new Prompt(promptId, ImmutableArray<Control>.Empty);

            Assert.Equal(promptId, prompt.PromptId);
        }

        [Fact, Trait("Type", "Unit")]
        public void PromptIdRoundTripsSerialization()
        {
            var promptId = Guid.NewGuid();
            var prompt = new Prompt(promptId, ImmutableArray<Control>.Empty);

            prompt = JsonConvert.DeserializeObject<Prompt>(JsonConvert.SerializeObject(prompt));

            Assert.Equal(promptId, prompt.PromptId);
        }

        [Fact, Trait("Type", "Unit")]
        public void ControlsSet()
        {
            var prompt = new Prompt(Guid.NewGuid(), ImmutableArray.Create
            (
                new Control("Test ButtonId A", "Test LeftIcon A", "Test LeftText A", "Test MiddleText A", "Test RightText A", "Test RightIcon A", true, false),
                new Control("Test ButtonId B", "Test LeftIcon B", "Test LeftText B", "Test MiddleText B", "Test RightText B", "Test RightIcon B", true, true),
                new Control("Test ButtonId C", "Test LeftIcon C", "Test LeftText C", "Test MiddleText C", "Test RightText C", "Test RightIcon C", false, true)
            ));

            Assert.Equal(3, prompt.Controls.Length);
            Assert.Equal("Test ButtonId A", prompt.Controls[0].ButtonId);
            Assert.Equal("Test LeftIcon A", prompt.Controls[0].LeftIcon);
            Assert.Equal("Test LeftText A", prompt.Controls[0].LeftText);
            Assert.Equal("Test MiddleText A", prompt.Controls[0].MiddleText);
            Assert.Equal("Test RightText A", prompt.Controls[0].RightText);
            Assert.Equal("Test RightIcon A", prompt.Controls[0].RightIcon);
            Assert.True(prompt.Controls[0].IsFirstInGroup);
            Assert.False(prompt.Controls[0].IsLastInGroup);
            Assert.Equal("Test ButtonId B", prompt.Controls[1].ButtonId);
            Assert.Equal("Test LeftIcon B", prompt.Controls[1].LeftIcon);
            Assert.Equal("Test LeftText B", prompt.Controls[1].LeftText);
            Assert.Equal("Test MiddleText B", prompt.Controls[1].MiddleText);
            Assert.Equal("Test RightText B", prompt.Controls[1].RightText);
            Assert.Equal("Test RightIcon B", prompt.Controls[1].RightIcon);
            Assert.True(prompt.Controls[1].IsFirstInGroup);
            Assert.True(prompt.Controls[1].IsLastInGroup);
            Assert.Equal("Test ButtonId C", prompt.Controls[2].ButtonId);
            Assert.Equal("Test LeftIcon C", prompt.Controls[2].LeftIcon);
            Assert.Equal("Test LeftText C", prompt.Controls[2].LeftText);
            Assert.Equal("Test MiddleText C", prompt.Controls[2].MiddleText);
            Assert.Equal("Test RightText C", prompt.Controls[2].RightText);
            Assert.Equal("Test RightIcon C", prompt.Controls[2].RightIcon);
            Assert.False(prompt.Controls[2].IsFirstInGroup);
            Assert.True(prompt.Controls[2].IsLastInGroup);
        }

        [Fact, Trait("Type", "Unit")]
        public void ControlsRoundTripsSerialization()
        {
            var prompt = new Prompt(Guid.NewGuid(), ImmutableArray.Create
            (
                new Control("Test ButtonId A", "Test LeftIcon A", "Test LeftText A", "Test MiddleText A", "Test RightText A", "Test RightIcon A", true, false),
                new Control("Test ButtonId B", "Test LeftIcon B", "Test LeftText B", "Test MiddleText B", "Test RightText B", "Test RightIcon B", true, true),
                new Control("Test ButtonId C", "Test LeftIcon C", "Test LeftText C", "Test MiddleText C", "Test RightText C", "Test RightIcon C", false, true)
            ));

            prompt = JsonConvert.DeserializeObject<Prompt>(JsonConvert.SerializeObject(prompt));

            Assert.Equal(3, prompt.Controls.Length);
            Assert.Equal("Test ButtonId A", prompt.Controls[0].ButtonId);
            Assert.Equal("Test LeftIcon A", prompt.Controls[0].LeftIcon);
            Assert.Equal("Test LeftText A", prompt.Controls[0].LeftText);
            Assert.Equal("Test MiddleText A", prompt.Controls[0].MiddleText);
            Assert.Equal("Test RightText A", prompt.Controls[0].RightText);
            Assert.Equal("Test RightIcon A", prompt.Controls[0].RightIcon);
            Assert.True(prompt.Controls[0].IsFirstInGroup);
            Assert.False(prompt.Controls[0].IsLastInGroup);
            Assert.Equal("Test ButtonId B", prompt.Controls[1].ButtonId);
            Assert.Equal("Test LeftIcon B", prompt.Controls[1].LeftIcon);
            Assert.Equal("Test LeftText B", prompt.Controls[1].LeftText);
            Assert.Equal("Test MiddleText B", prompt.Controls[1].MiddleText);
            Assert.Equal("Test RightText B", prompt.Controls[1].RightText);
            Assert.Equal("Test RightIcon B", prompt.Controls[1].RightIcon);
            Assert.True(prompt.Controls[1].IsFirstInGroup);
            Assert.True(prompt.Controls[1].IsLastInGroup);
            Assert.Equal("Test ButtonId C", prompt.Controls[2].ButtonId);
            Assert.Equal("Test LeftIcon C", prompt.Controls[2].LeftIcon);
            Assert.Equal("Test LeftText C", prompt.Controls[2].LeftText);
            Assert.Equal("Test MiddleText C", prompt.Controls[2].MiddleText);
            Assert.Equal("Test RightText C", prompt.Controls[2].RightText);
            Assert.Equal("Test RightIcon C", prompt.Controls[2].RightIcon);
            Assert.False(prompt.Controls[2].IsFirstInGroup);
            Assert.True(prompt.Controls[2].IsLastInGroup);
        }

        [Fact, Trait("Type", "Unit")]
        public void BackgroundLayersDefaultsToEmpty()
        {
            var prompt = new Prompt(Guid.NewGuid(), ImmutableArray<Control>.Empty);

            Assert.Empty(prompt.BackgroundLayers);
        }

        [Fact, Trait("Type", "Unit")]
        public void BackgroundLayersSet()
        {
            var prompt = new Prompt(Guid.NewGuid(), ImmutableArray<Control>.Empty, ImmutableArray.Create("Test Background Layer A", "Test Background Layer B", "Test Background Layer C"));

            Assert.Equal(new[] { "Test Background Layer A", "Test Background Layer B", "Test Background Layer C" }, prompt.BackgroundLayers);
        }

        [Fact, Trait("Type", "Unit")]
        public void BackgroundLayersRoundTripsSerialization()
        {
            var prompt = new Prompt(Guid.NewGuid(), ImmutableArray<Control>.Empty, ImmutableArray.Create("Test Background Layer A", "Test Background Layer B", "Test Background Layer C"));

            prompt = JsonConvert.DeserializeObject<Prompt>(JsonConvert.SerializeObject(prompt));

            Assert.Equal(new[] { "Test Background Layer A", "Test Background Layer B", "Test Background Layer C" }, prompt.BackgroundLayers);
        }
    }
}
