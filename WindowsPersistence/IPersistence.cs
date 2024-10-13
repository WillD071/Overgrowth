public interface IPersistence {
  
    // check if binary is running at the beginning and end of the script. Maybe multithread?
    // add plenty of error handling to ensure it always runs
    static abstract void makeUndeletable();
    static abstract void setSilentProcessExit();
    static abstract void setScheduledTasks();
    static abstract void SetKeyboardShortcuts();
    static abstract void setWinLogonKey();
    static abstract void makeImageFileExecution(); // Updated as per your comment for IFEO.

    // Newly added methods based on URLs provided
    static abstract void makeWindowsLoadKey();        // For windowsload.html
    static abstract void makeServicesKey();           // For services.html
    static abstract void makeAeDebugKey();            // For aedebug.html
    static abstract void makeWerDebuggerKey();        // For wer_debugger.html
    static abstract void makeNaturalLanguageKey();    // For naturallanguage6.html
    static abstract void makeDiskCleanupHandler();    // For diskcleanuphandler.html
    static abstract void makeHtmlHelpAuthorKey();     // For htmlhelpauthor.html
    static abstract void makeHhctrlKey();             // For hhctrl.html
    static abstract void makeAmsiKey();               // For amsi.html
    static abstract void makeServerLevelPluginDll();   // For serverlevelplugindll.html
    static abstract void makePasswordFilter();        // For passwordfilter.html
    static abstract void makeCredManDll();            // For credmandll.html
    static abstract void makeAuthenticationPackages();   // For authenticationpackages.html
    static abstract void makeCodeSigning();           // For codesigning.html
    static abstract void makeCmdAutoRun();            // For cmdautorun.html
    static abstract void makeLsaAExtension();         // For lsaaextension.html
    static abstract void makeMpNotify();              // For mpnotify.html
    static abstract void makeExplorerTools();         // For explorertools.html
    static abstract void makeWindowsTerminalProfile();   // For windowsterminalprofile.html
    static abstract void makeStartupFolder();         // For startupfolder.html
    static abstract void makeAutoDialDll();           // For autodialdll.html
    static abstract void makeTsInitialProgram();      // For tsinitialprogram.html
    static abstract void makeIFilter();               // For ifilter.html
    static abstract void makeRecycleBin();            // For recyclebin.html
    static abstract void makeTelemetryController();   // For telemetrycontroller.html
    static abstract void makeSilentExitMonitor();     // For silentexitmonitor.html
    static abstract void makeScreenSaver();           // For screensaver.html
    static abstract void makeBootVerificationProgram();   // For bootverificationprogram.html
    static abstract void makeFileExtensionHijacking();   // For fileextensionhijacking.html
    static abstract void makeKeyboardShortcut();      // For keyboardshortcut.html

    // Added for scripts as well
    static abstract void makePowerShellProfile();     // For powershellprofile.html
    static abstract void makeUserInitMprLogonScript();   // For userinitmprlogonscript.html
}
    /**
    Keys:
    https://persistence-info.github.io/Data/windowsload.html
    https://persistence-info.github.io/Data/services.html
    https://persistence-info.github.io/Data/aedebug.html
    https://persistence-info.github.io/Data/wer_debugger.html
    https://persistence-info.github.io/Data/naturallanguage6.html
    https://persistence-info.github.io/Data/diskcleanuphandler.html
    https://persistence-info.github.io/Data/htmlhelpauthor.html
    https://persistence-info.github.io/Data/hhctrl.html
    https://persistence-info.github.io/Data/amsi.html
    https://persistence-info.github.io/Data/serverlevelplugindll.html
    https://persistence-info.github.io/Data/passwordfilter.html
    https://persistence-info.github.io/Data/credmandll.html
    https://persistence-info.github.io/Data/authenticationpackages.html
    https://persistence-info.github.io/Data/codesigning.html
    https://persistence-info.github.io/Data/cmdautorun.html
    https://persistence-info.github.io/Data/lsaaextension.html
    https://persistence-info.github.io/Data/mpnotify.html
    https://persistence-info.github.io/Data/explorertools.html
    https://persistence-info.github.io/Data/windowsterminalprofile.html
    https://persistence-info.github.io/Data/startupfolder.html
    https://persistence-info.github.io/Data/autodialdll.html
    https://persistence-info.github.io/Data/tsinitialprogram.html
    https://persistence-info.github.io/Data/ifilter.html
    https://persistence-info.github.io/Data/recyclebin.html
    https://persistence-info.github.io/Data/telemetrycontroller.html
    https://persistence-info.github.io/Data/silentexitmonitor.html
    https://persistence-info.github.io/Data/screensaver.html
    https://persistence-info.github.io/Data/bootverificationprogram.html
    https://persistence-info.github.io/Data/fileextensionhijacking.html
    https://persistence-info.github.io/Data/keyboardshortcut.html
     ***/


    /*
       scripts
       https://persistence-info.github.io/Data/powershellprofile.html
       https://persistence-info.github.io/Data/userinitmprlogonscript.html

    */
