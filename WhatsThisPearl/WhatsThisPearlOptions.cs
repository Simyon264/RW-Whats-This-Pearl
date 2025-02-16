using Menu.Remix.MixedUI;
using UnityEngine;

namespace WhatsThisPearl;

public class WhatsThisPearlOptions : OptionInterface
{
    private WhatsThisPearl _mod;

    public readonly Configurable<bool> ColorLabels;
    //public readonly Configurable<bool> ShowHasRead;
    public readonly Configurable<bool> ShowPearlID;

    private UIelement[] _uiArrPlayerOptions;

    public WhatsThisPearlOptions(WhatsThisPearl mod)
    {
        _mod = mod;

        ColorLabels = config.Bind<bool>("ColorLabels", true);
        //ShowHasRead = config.Bind<bool>("ShowHasRead", true);
        ShowPearlID = config.Bind<bool>("ShowPearlID", false);
    }

    public override void Initialize()
    {
        base.Initialize();

        var tab = new OpTab(this, "Options");

        Tabs = [tab];

        _uiArrPlayerOptions = new UIelement[]
        {
            new OpLabel(10f, 550f, "Options", true),
            new OpLabel(10f, 520f, "Color label based on pearl type"),
            new OpCheckBox(ColorLabels, new Vector2(10f,490f)),
            new OpLabel(10f, 460f, "Show pearl ID"),
            new OpCheckBox(ShowPearlID, new Vector2(10f, 430f)),
        };

        tab.AddItems(_uiArrPlayerOptions);
    }
}