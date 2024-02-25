using MasterServerToolkit.Bridges;
using MasterServerToolkit.MasterServer;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class ProfileSettingsView : MasterServerToolkit.Examples.BasicProfile.ProfileSettingsView
{
    public FlexibleColorPicker flexibleColorPicker;

    public void ColorChangeBtn()
    {
        flexibleColorPicker.gameObject.SetActive(true);
        Debug.Log(profileBehaviour.Profile.Get<ObservableColor>(ProfilePropertyOpCodes.tankColor).Value.ToString());
        flexibleColorPicker.SetColor(profileBehaviour.Profile.Get<ObservableColor>(ProfilePropertyOpCodes.tankColor).Value);
    }

    public override void Submit()
    {
        var data = new MstProperties();
        data.Set("displayName", displayNameInputField.text);
        data.Set("avatarUrl", avatarUrlInputField.text);
        data.Set("tankColor", "#" + ColorUtility.ToHtmlStringRGBA(flexibleColorPicker.GetColor()));

        Debug.Log("Select Color " + flexibleColorPicker.GetColor());

        profileBehaviour.UpdateProfile(data);
    }

}
