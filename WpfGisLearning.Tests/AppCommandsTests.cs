using System.Windows.Input;

namespace WpfGisLearning.Tests;

[TestClass]
public class AppCommandsTests
{
    [TestMethod]
    public void SaveCommand_HasExpectedMetadata()
    {
        Assert.AreEqual("Save", AppCommands.SaveCommand.Text);
        Assert.AreEqual("Save", AppCommands.SaveCommand.Name);
        Assert.AreEqual(typeof(AppCommands), AppCommands.SaveCommand.OwnerType);
    }

    [TestMethod]
    public void SaveCommand_UsesCtrlSGesture()
    {
        var gesture = AppCommands.SaveCommand.InputGestures.OfType<KeyGesture>().Single();

        Assert.AreEqual(Key.S, gesture.Key);
        Assert.AreEqual(ModifierKeys.Control, gesture.Modifiers);
    }
}
