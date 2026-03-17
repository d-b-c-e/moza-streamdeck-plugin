using Xunit;
using MozaStreamDeck.Core.Profiles;

namespace MozaStreamDeck.Tests;

public class PresetManagerTests
{
    [Fact]
    public void GetMotorPresets_ReturnsNonEmptyList_WhenPitHouseInstalled()
    {
        // This test only passes if Moza Pit House is installed with motor presets
        var presets = PresetManager.GetMotorPresets();

        // Skip if Pit House not installed
        if (PresetManager.FindPitHouseDirectory() == null)
            return;

        Assert.NotEmpty(presets);
    }

    [Fact]
    public void GetMotorPresets_AllPresetsHaveNames()
    {
        var presets = PresetManager.GetMotorPresets();

        foreach (var preset in presets)
        {
            Assert.False(string.IsNullOrWhiteSpace(preset.Name),
                $"Preset at {preset.FilePath} has empty name");
        }
    }

    [Fact]
    public void GetMotorPresets_SortedAlphabetically()
    {
        var presets = PresetManager.GetMotorPresets();
        if (presets.Count < 2) return;

        for (int i = 1; i < presets.Count; i++)
        {
            Assert.True(
                string.Compare(presets[i - 1].Name, presets[i].Name,
                    StringComparison.InvariantCultureIgnoreCase) <= 0,
                $"Presets not sorted: '{presets[i - 1].Name}' should come before '{presets[i].Name}'");
        }
    }

    [Fact]
    public void GetMotorPresets_AllPresetsHaveFilePaths()
    {
        var presets = PresetManager.GetMotorPresets();

        foreach (var preset in presets)
        {
            Assert.False(string.IsNullOrWhiteSpace(preset.FilePath),
                $"Preset '{preset.Name}' has empty file path");
            Assert.True(File.Exists(preset.FilePath),
                $"Preset '{preset.Name}' file does not exist: {preset.FilePath}");
        }
    }

    [Fact]
    public void FindPitHouseDirectory_ReturnsNullOrValidPath()
    {
        var dir = PresetManager.FindPitHouseDirectory();

        if (dir != null)
        {
            Assert.True(Directory.Exists(dir), $"Returned directory does not exist: {dir}");
            Assert.True(Directory.Exists(Path.Combine(dir, "Presets", "Motor")),
                $"Motor presets subdirectory missing in: {dir}");
        }
        // null is also valid (Pit House not installed)
    }
}
