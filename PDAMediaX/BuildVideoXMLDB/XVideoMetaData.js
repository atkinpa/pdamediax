WScript.Echo("Length" + WScript.Arguments.length);
if (WScript.FullName.search(/wscript\.exe/i) != -1 || WScript.Arguments.length == 0) {
    Run (WScript.Arguments(0));
}
else {
    WScript.Echo("Usage: cscript XVideoMetaData.js <scan-folder>");
}
function Run(path) {
    var shell = new ActiveXObject("Shell.Application");
    var folder = shell.Namespace(path);

    for (var i = 0; i < folder.Items().Count; i++) {
        var item = folder.Items().Item(i);
        var recording = new Recording(item);

        WScript.Echo("");
        WScript.Echo("Recording.Path");
        WScript.Echo("*************************************************************");
        WScript.Echo("Title                   :" + recording.Title);
        WScript.Echo("Sub-Title               :" + recording.SubTitle);
        WScript.Echo("Episode Name            :" + recording.EpisodeName);
        WScript.Echo("Network Affiliation     :" + recording.NetworkAffiliation);
        WScript.Echo("Channel Number          :" + recording.ChannelNumber);
        WScript.Echo("Station Name            :" + recording.StationName);
        WScript.Echo("Station Call Sign       :" + recording.StationCallSign);
        WScript.Echo("Program Description     :" + recording.ProgramDescription);
        WScript.Echo("Credits                 :" + recording.Credits);
        WScript.Echo("Original Broadcast Date :" + recording.OriginaBroadcastDate);
        WScript.Echo("Recordint Time          :" + recording.RecordingTime);
        WScript.Echo("Is HD                   :" + recording.IsHDContent);
        WScript.Echo("Is DTV                  :" + recording.IsDTVContent);
        WScript.Echo("Is Repeat Broadcast     :" + recording.IsRepeatBroadcast);
        WScript.Echo("Year                    :" + recording.Year);
        WScript.Echo("Duration                :" + recording.Duration);
    }
}
function Recording (folderitem) {
    this.Path = folderitem.Path;
    this.Title = folderitem.ExtendedProperty("System.Title");
    this.SubTitle = folderitem.ExtendedProperty("System.Media.SubTitle");
    this.EpisodeName = folderitem.ExtendedProperty("System.RecordedTV.EpisodeName");
    this.NetworkAffiliation = folderitem.ExtendedProperty("System.RecordedTV.NetworkAffiliation");
    this.ChannelNumber = folderitem.ExtendedProperty("System.RecordedTV.ChannelNumber");
    this.StationName = folderitem.ExtendedProperty("System.RecordedTV.StationName");
    this.StationCallSign = folderitem.ExtendedProperty("System.RecordedTV.StationCallSign");
    this.ProgramDescription = folderitem.ExtendedProperty("System.RecordedTV.ProgramDescription");
    this.Credits = folderitem.ExtendedProperty("System.RecordedTV.Credits");
    this.OriginalBroadcastDate = folderitem.ExtendedProperty("System.RecordedTV.OriginalBroadcastDate");
    this.RecordingTime = folderitem.ExtendedProperty("System.RecordedTV.RecordingTime");
    this.IsHDContent = folderitem.ExtendedProperty("System.RecordedTV.IsHDContent");
    this.IsDTVContent = folderitem.ExtendedProperty("System.RecordedTV.IsDTVContent");
    this.IsRepeatBroadcast = folderitem.ExtendedProperty("System.RecordedTV.IsRepeatBroadcast");
    this.Year = folderitem.ExtendedProperty("System.Media.Year");
    this.Duration = folderitem.ExtendedProperty("System.Media.Duration");
}