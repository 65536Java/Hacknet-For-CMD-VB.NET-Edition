Imports Engine
Imports Entropy.System
Imports System
Public Class ForkBomb
    Inherits Process
    Public Sub New()
        needRam = 1
    End Sub
    Public Overrides Sub ProcessMain()
        Console.WriteLine(RandomString01(10))
        Dim newProcess As New ForkBomb()
        Process.StartProcess(newProcess, Game.GetMaxRam(), Game.GetUsedRam())
        While True
        End While
    End Sub
    Function RandomString01(length As Integer) As String
        Dim random As New Random()
        Dim chars As String = "01"
        Dim result As New System.Text.StringBuilder(length)
        For i As Integer = 1 To length
            Dim index As Integer = random.Next(chars.Length)
            result.Append(chars(index))
        Next
        Return result.ToString()
    End Function
End Class