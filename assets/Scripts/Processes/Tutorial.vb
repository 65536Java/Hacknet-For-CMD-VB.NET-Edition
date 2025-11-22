' ...existing code...
Imports System
Imports System.IO
Imports Server
Imports Engine
Imports Audio
Imports Entropy.System
Imports System.Linq
Imports Terminals

Public Class Tutorial
    Inherits Process
    Dim ServersAvailable As HNServer()
    Public Sub New()
        PID = 12
        Name = "Tutorial"
        needRam = 500
    End Sub
    Public Overrides Sub ProcessMain()
        Dim temp As String
        AudioUtil.GetSoundPlayer(Game.BaseDir,"assets\audios\¡A Todo Plan!.wav").PlayLooping()
        Dim list = New ServerInfoParse().LoadServers(Path.Combine("assets", "Nodes", "Tutorial.json"))
        If list Is Nothing Then
            ServersAvailable = New HNServer() {}
        Else
            ServersAvailable = list.ToArray()
        End If
        ' 把伺服器加入 Game 並在此輸出 debug（確保 ServersAvailable 已初始化）
        For Each server As HNServer In ServersAvailable
            Game.ServersAvailable.Add(server)
        Next
        Console.Clear()
        Console.WriteLine("Tutorial:")
        Console.WriteLine("As of right now you are at risk!")
        Console.WriteLine("Learn as quickly as possible.")
        Console.WriteLine("Begin the tutorial sequence by pressing any key.")
        Console.ReadKey(True)
        Console.Clear()
        Console.WriteLine("Tutorial:")
        Console.WriteLine("Connect to a computer by typing connect [IP] in the terminal.")
        Console.WriteLine("Now, connect to your computer.")
        Console.WriteLine("Your Computer's IP address is 116.121.4.6")
        Do
            temp = Terminal.terminal(ServersAvailable, CurrentComputer)
        Loop While temp <> "connect" AndAlso CurrentComputer.IP <> "116.121.4.6"
        Console.WriteLine("Tutorial:")
        Console.WriteLine("Good work.")
        Console.WriteLine("The first thing to do on any system is scan it for adjacent nodes.")
        Console.WriteLine("This will reveal more computers on your network that you can use.")
        Console.WriteLine("Scan this computer now by typing scan.")
        Do
            temp = Terminal.terminal(ServersAvailable, CurrentComputer)
        Loop While temp <> "scan"
        Console.WriteLine("Tutorial:")
        Console.WriteLine("That should be all you'll need from your own server for now.")
        Console.WriteLine("Disconnect from your machine by typing dc or disconnect.")
        Do
            temp = Terminal.terminal(ServersAvailable, CurrentComputer)
        Loop While temp <> "dc" AndAlso temp <> "disconnect"
        Console.WriteLine("Tutorial:")
        Console.WriteLine("It's time for you to connect to an outside computer.")
        Console.WriteLine("Be aware that attempting to compromise the security of another's computer is illegal under the U.S.C. Act 1030-18.")
        Console.WriteLine("Proceed at your own risk and connect to an outside machine by typing connect [IP].")
        Do
            temp = Terminal.terminal(ServersAvailable, CurrentComputer)
        Loop While temp <> "connect" AndAlso CurrentComputer.IP <> "211.467.8.6"
        Console.WriteLine("Tutorial:")
        Console.WriteLine("This VM's Terminal module has been activated. This will be your primary interface for navigating and interacting with nodes.")
        Console.WriteLine("A command can be run by typing it out and pressing Enter.")
        Console.WriteLine("A computer's security system and open ports can be analyzed using the probe or nmap command.")
        Console.WriteLine("Analyze the computer you are currently connected to.")
        Do
            temp = Terminal.terminal(ServersAvailable, CurrentComputer)
        Loop While temp <> "probe" AndAlso temp <> "nmap"
        Console.WriteLine("Tutorial:")
        Console.WriteLine("Here you can see the active ports, active security, and the number of open ports required to successfully crack this machine using porthack.")
        Console.WriteLine("This machine has no active security and requires no open ports to crack.")
        Console.WriteLine("If you're prepared, it's possible to crack this computer using the porthack.")
        Console.WriteLine("Run the program porthack ")
        Do
            temp = Terminal.terminal(ServersAvailable, CurrentComputer)
        Loop While temp <> "porthack" 
        Console.WriteLine("Tutorial:")
        Console.WriteLine("Congratulations, You have taken control of an external system and are now it's administrator.")
        Console.WriteLine("You can do whatever you like with it, however you should start by Scanning for local nodes to locate additional computers.")
        Console.WriteLine("Do this using the Scan command.")
        Do
            temp = Terminal.terminal(ServersAvailable, CurrentComputer)
        Loop While temp <> "scan"
        Console.WriteLine("Tutorial:")
        Console.WriteLine("No results - not a problem.")
        Console.WriteLine("Next, you should investigate the filesystem.")
        Console.WriteLine("List the files using the ls command.")
        Do
            temp = Terminal.terminal(ServersAvailable, CurrentComputer)
        Loop While temp <> "ls"
        Console.WriteLine("Tutorial:")
        Console.WriteLine("Navigate to bin folder(Binaries Folder) to search for useful executable using the command.")
        Console.WriteLine("cd [FOLDER NAME]")
        Do
            temp = Terminal.terminal(ServersAvailable, CurrentComputer)
        Loop While temp <> "cd" OrElse Game.CurrentPath <> "bin"
        Console.WriteLine("Tutorial:")
        Console.WriteLine("To view contents of the current folder you're in use the command ls.")
        Console.WriteLine("These are no programs here, but you should look at config.txt")
        Console.WriteLine("Use the cat [file] to view the contents of a file.")
        Do
            temp = Terminal.terminal(ServersAvailable, CurrentComputer)
        Loop While temp <> "cat"
        Console.WriteLine("Tutorial:")
        Console.WriteLine("Totally useless!")
        Console.WriteLine("Now clear your tracks before you leave.")
        Console.WriteLine("Use the command cd .. to go back to the previous directory.")
        Do
            temp = Terminal.terminal(ServersAvailable, CurrentComputer)
        Loop While temp <> "cd" OrElse Game.CurrentPath <> ""
        Console.WriteLine("Tutorial:")
        Console.WriteLine("Move to the log folder.")
        Console.WriteLine("cd [FOLDER NAME]")
        Do
            temp = Terminal.terminal(ServersAvailable, CurrentComputer)
        Loop While temp <> "cd" OrElse Game.CurrentPath <> "log"
        Console.WriteLine("Tutorial:")
        Console.WriteLine("Delete all files in this directory.")
        Console.WriteLine("Use the command rm [file] to delete a file.")
        Console.WriteLine("Note:The wildcard * indicates All.")
        Do
            temp = Terminal.terminal(ServersAvailable, CurrentComputer)
        Loop While temp <> "rm" OrElse Game.CurrentPath <> "log"
        Console.WriteLine("Tutorial:")
        Console.WriteLine("Excellent work.")
        Console.WriteLine("Disconnect from this computer.")
        Console.WriteLine("You can use the dc or disconnect command.")
        Do
            temp = Terminal.terminal(ServersAvailable, CurrentComputer)
        Loop While temp <> "dc" AndAlso temp <> "disconnect"
        Console.WriteLine("Tutorial:")
        Console.WriteLine("Congratulations, you have completed the guided section of this tutorial.")
        Console.WriteLine("To finish it, you must locate the Process id of this tutorial process.")
        Console.WriteLine("Use kill [Process ID] to kill a process And Use ps to list processes.")
        Do
            temp = Terminal.terminal(ServersAvailable, CurrentComputer)
        Loop While temp <> "kill"
    End Sub
End Class
' ...existing code...