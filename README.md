cd C:\Windows\Microsoft.NET\Framework64\v4.0.30319

Install Service(... - path to service):

InstallUtil.exe ...\FTPTargetWatcher.exe

Start with config:

sc start FTPTargetWatcher ...\*.json

Uninstall:

InstallUtil.exe /u ...\FTPTargetWatcher.exe
