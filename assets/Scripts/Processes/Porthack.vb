Imports System.Threading
Imports System
Imports System.Random
Imports Entropy.System
Public Class Porthack
    Inherits Process
    Dim Computer As HNServer
    Public Sub New(server As HNServer)
        Name = "Porthack"
        Computer = server
        needRam = 246
    End Sub
    Public Overrides Sub ProcessMain()
        If Computer.CrackedPorts = Computer.NeedCrackPortsCount Then
            Console.WriteLine("Porthack initialized -- Running...")
            For i As Integer = 1 To 100
                Dim str As String = RandomString(10)
                Thread.Sleep(50)
                Console.WriteLine(str & "          " & RandomString(10))
            Next
            Computer.IsAdmin = True
            Console.WriteLine("Porthack complete - Password Found.")
        Else
            Console.WriteLine("Porthack failed - Not enough ports cracked.")
        End If
        Kill()
    End Sub
    Function RandomString(length As Integer) As String
            Dim random As New Random()
            Dim chars As String = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"
            Dim result As New System.Text.StringBuilder(length)
            For i As Integer = 1 To length
                Dim index As Integer = random.Next(chars.Length)
                result.Append(chars(index))
            Next
            Return result.ToString()
        End Function
End Class