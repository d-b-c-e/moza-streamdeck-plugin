using Xunit;
using MozaStreamDeck.Core.Profiles;

namespace MozaStreamDeck.Tests;

public class PresetProfileTests
{
    private readonly string _testDir;

    public PresetProfileTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), "MozaTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDir);
    }

    private string WritePresetFile(string json, string filename = "test.json")
    {
        var path = Path.Combine(_testDir, filename);
        File.WriteAllText(path, json);
        return path;
    }

    [Fact]
    public void LoadFromFile_ValidPreset_ParsesAllFields()
    {
        var path = WritePresetFile("""
        {
            "id": "abc-123",
            "name": "My Preset",
            "devices": ["R12", "R16"],
            "deviceParams": {
                "gameForceFeedbackStrength": 75,
                "maximumSteeringAngle": 540,
                "mechanicalDamper": 30
            }
        }
        """);

        var preset = PresetProfile.LoadFromFile(path);

        Assert.NotNull(preset);
        Assert.Equal("abc-123", preset.Id);
        Assert.Equal("My Preset", preset.Name);
        Assert.Equal(path, preset.FilePath);
        Assert.Equal(new List<string> { "R12", "R16" }, preset.Devices);
        Assert.Equal(3, preset.DeviceParams.Count);
        // Numeric params are stored as double due to C# ternary type promotion;
        // ApplyPreset uses Convert.ToInt32() to consume them
        Assert.Equal(75, Convert.ToInt32(preset.DeviceParams["gameForceFeedbackStrength"]));
        Assert.Equal(540, Convert.ToInt32(preset.DeviceParams["maximumSteeringAngle"]));
        Assert.Equal(30, Convert.ToInt32(preset.DeviceParams["mechanicalDamper"]));
    }

    [Fact]
    public void LoadFromFile_BooleanParams_ParsedCorrectly()
    {
        var path = WritePresetFile("""
        {
            "id": "bool-test",
            "name": "Bool Test",
            "deviceParams": {
                "gameForceFeedbackReversal": true,
                "safeDrivingEnabled": false
            }
        }
        """);

        var preset = PresetProfile.LoadFromFile(path);

        Assert.NotNull(preset);
        Assert.Equal(true, preset.DeviceParams["gameForceFeedbackReversal"]);
        Assert.Equal(false, preset.DeviceParams["safeDrivingEnabled"]);
    }

    [Fact]
    public void LoadFromFile_FloatParam_ParsedAsDouble()
    {
        var path = WritePresetFile("""
        {
            "id": "float-test",
            "name": "Float Test",
            "deviceParams": {
                "someFloatValue": 3.14
            }
        }
        """);

        var preset = PresetProfile.LoadFromFile(path);

        Assert.NotNull(preset);
        Assert.IsType<double>(preset.DeviceParams["someFloatValue"]);
        Assert.Equal(3.14, preset.DeviceParams["someFloatValue"]);
    }

    [Fact]
    public void LoadFromFile_StringParam_ParsedCorrectly()
    {
        var path = WritePresetFile("""
        {
            "id": "str-test",
            "name": "String Test",
            "deviceParams": {
                "someStringValue": "hello"
            }
        }
        """);

        var preset = PresetProfile.LoadFromFile(path);

        Assert.NotNull(preset);
        Assert.Equal("hello", preset.DeviceParams["someStringValue"]);
    }

    [Fact]
    public void LoadFromFile_MissingFields_DefaultsToEmpty()
    {
        var path = WritePresetFile("{}");

        var preset = PresetProfile.LoadFromFile(path);

        Assert.NotNull(preset);
        Assert.Equal("", preset.Id);
        Assert.Equal("", preset.Name);
        Assert.Empty(preset.Devices);
        Assert.Empty(preset.DeviceParams);
    }

    [Fact]
    public void LoadFromFile_EmptyDeviceParams_ReturnsEmptyDict()
    {
        var path = WritePresetFile("""
        {
            "id": "empty-params",
            "name": "Empty",
            "deviceParams": {}
        }
        """);

        var preset = PresetProfile.LoadFromFile(path);

        Assert.NotNull(preset);
        Assert.Empty(preset.DeviceParams);
    }

    [Fact]
    public void LoadFromFile_InvalidJson_ReturnsNull()
    {
        var path = WritePresetFile("this is not json at all {{{");

        var preset = PresetProfile.LoadFromFile(path);

        Assert.Null(preset);
    }

    [Fact]
    public void LoadFromFile_NonexistentFile_ReturnsNull()
    {
        var preset = PresetProfile.LoadFromFile(@"C:\nonexistent\path\fake.json");

        Assert.Null(preset);
    }

    [Fact]
    public void LoadFromFile_EmptyDevicesArray_ReturnsEmptyList()
    {
        var path = WritePresetFile("""
        {
            "id": "no-devices",
            "name": "No Devices",
            "devices": []
        }
        """);

        var preset = PresetProfile.LoadFromFile(path);

        Assert.NotNull(preset);
        Assert.Empty(preset.Devices);
    }

    [Fact]
    public void LoadFromFile_RealWorldPreset_ParsesCorrectly()
    {
        // Mimics the structure of actual Pit House motor preset files
        var path = WritePresetFile("""
        {
            "id": "3e4d0e5e-0af8-4c84-8588-346af5669f3b",
            "name": "_Arcade",
            "devices": ["R12"],
            "deviceParams": {
                "gameForceFeedbackStrength": 50,
                "maximumSteeringAngle": 360,
                "maximumGameSteeringAngle": 360,
                "maximumTorque": 100,
                "mechanicalDamper": 0,
                "mechanicalSpringStrength": 50,
                "mechanicalFriction": 0,
                "maximumSteeringSpeed": 100,
                "gameForceFeedbackReversal": false,
                "speedDependentDamping": 0,
                "initialSpeedDependentDamping": 0,
                "safeDrivingEnabled": false,
                "safeDrivingMode": 0,
                "gameForceFeedbackFilter": 1,
                "softLimitStrength": 100,
                "naturalInertiaV2": 100
            }
        }
        """);

        var preset = PresetProfile.LoadFromFile(path);

        Assert.NotNull(preset);
        Assert.Equal("_Arcade", preset.Name);
        Assert.Equal("3e4d0e5e-0af8-4c84-8588-346af5669f3b", preset.Id);
        Assert.Single(preset.Devices);
        Assert.Equal("R12", preset.Devices[0]);

        // Verify numeric params (Convert.ToInt32 matches how ApplyPreset consumes them)
        Assert.Equal(50, Convert.ToInt32(preset.DeviceParams["gameForceFeedbackStrength"]));
        Assert.Equal(360, Convert.ToInt32(preset.DeviceParams["maximumSteeringAngle"]));
        Assert.Equal(360, Convert.ToInt32(preset.DeviceParams["maximumGameSteeringAngle"]));
        Assert.Equal(100, Convert.ToInt32(preset.DeviceParams["maximumTorque"]));
        Assert.Equal(0, Convert.ToInt32(preset.DeviceParams["mechanicalDamper"]));

        // Verify boolean params
        Assert.Equal(false, preset.DeviceParams["gameForceFeedbackReversal"]);
        Assert.Equal(false, preset.DeviceParams["safeDrivingEnabled"]);

        // Verify unsupported params still parse (they're in the dict, just ignored by ApplyPreset)
        Assert.True(preset.DeviceParams.ContainsKey("gameForceFeedbackFilter"));
        Assert.True(preset.DeviceParams.ContainsKey("naturalInertiaV2"));
    }

    [Fact]
    public void LoadFromFile_NumericParams_ConvertibleToInt()
    {
        // All JSON integer values should be convertible via Convert.ToInt32,
        // matching how ApplyPreset consumes them
        var path = WritePresetFile("""
        {
            "id": "convert-test",
            "name": "Convert Test",
            "deviceParams": {
                "mechanicalDamper": 0,
                "initialSpeedDependentDamping": 0,
                "gameForceFeedbackStrength": 100
            }
        }
        """);

        var preset = PresetProfile.LoadFromFile(path);

        Assert.NotNull(preset);
        Assert.Equal(0, Convert.ToInt32(preset.DeviceParams["mechanicalDamper"]));
        Assert.Equal(0, Convert.ToInt32(preset.DeviceParams["initialSpeedDependentDamping"]));
        Assert.Equal(100, Convert.ToInt32(preset.DeviceParams["gameForceFeedbackStrength"]));
    }

    [Fact]
    public void LoadFromFile_NullDeviceInArray_Skipped()
    {
        var path = WritePresetFile("""
        {
            "id": "null-device",
            "name": "Null Device",
            "devices": ["R12", null, "R16"]
        }
        """);

        var preset = PresetProfile.LoadFromFile(path);

        Assert.NotNull(preset);
        Assert.Equal(2, preset.Devices.Count);
        Assert.Equal("R12", preset.Devices[0]);
        Assert.Equal("R16", preset.Devices[1]);
    }
}
