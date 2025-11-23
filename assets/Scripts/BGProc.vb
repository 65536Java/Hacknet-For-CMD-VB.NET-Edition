Imports BCEngine
Imports Entropy.System
Imports System
Public Class BGProc
    Inherits Process
    Public Sub New()
        needRam = 0
    End Sub
    Public Overrides Sub ProcessMain()
        Do
            Threading.Thread.Sleep(100)
        Loop Until Game.GetUsedRam() >= Game.GetMaxRam()
        Game.BSoD()
    End Sub
End Class