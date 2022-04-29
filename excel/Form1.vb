Imports MySql.Data.MySqlClient
Imports System.IO.StreamWriter
Public Class Form1
    Public mysqlcon As MySqlConnection
    Public sda As New MySqlDataAdapter
    'Public constr = "server=192.168.1.10;userid=signet;password=enapass;database=dbkafu;port=33953;"
    Public constr = "server=91.138.207.140;userid=remote;password=enapassremote!@#;port=33953"
    Dim dateupdated As String
    Public command As MySqlCommand
    Dim query As String
    Dim keno, kwdikos, perigrafi, mikri_perigrafi, fpa, lianiki, barcode1, mm1, zugistikos_kwdikos As String
    Public trans As MySqlTransaction
    Public queryerror As Boolean = False
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        RadioButton1.Checked = True
    End Sub

    Dim fileEncoding As System.Text.Encoding = System.Text.Encoding.GetEncoding(28597)
    Private Sub gapfill(fieldlength As Integer, actuallength As Integer)
        keno = ""
        For i = 1 To fieldlength - actuallength
            keno = keno & " "

        Next
    End Sub
    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        If RadioButton1.Checked = True Then
            dateupdated = returnsinglevaluequery("select lastupdate from signet.app_remote")
            query = "select code,perigrafi,fpa,round(lianiki,2),case when barcode1 like '25%' then code else '' end,barcode1,mm1 from dbkafu.eidi where lianiki>0 and dateupdated> (select lastupdate from signet.app_remote)"

        ElseIf RadioButton2.Checked = True Then
            query = "select code,perigrafi,fpa,round(lianiki,2),case when barcode1 like '25%' then code else '' end,barcode1,mm1 from dbkafu.eidi where lianiki>0"

        End If
        Try
            mysqlcon = New MySqlConnection
            mysqlcon.ConnectionString = constr
            Dim sda As New MySqlDataAdapter
            Dim dt As New DataTable
            mysqlcon.Open()

            command = New MySqlCommand(query, mysqlcon)

            sda.SelectCommand = command
            sda.Fill(dt)
            If dt.Rows.Count = 0 Then
                MessageBox.Show("Δε βρέθηκαν ενημερωμένα είδη!" & vbCrLf & "Επιλέξτε εναλλακτικά όλα τα είδη.")
                Exit Sub
            End If
            Dim txt As String
            Dim fileloc As String = "C:\data\plu.txt"
            If System.IO.File.Exists(fileloc) Then
                System.IO.File.Delete(fileloc)
            End If

            Using sw As New System.IO.StreamWriter(fileloc, False, fileEncoding)
                For i = 0 To dt.Rows.Count - 1
                    Dim lst As New List(Of String)

                    kwdikos = dt.Rows(i).Item(0)
                    gapfill(12, kwdikos.Length)
                    kwdikos = kwdikos & keno
                    lst.Add(kwdikos)

                    mikri_perigrafi = dt.Rows(i).Item(1)
                    If mikri_perigrafi.Length > 25 Then
                        mikri_perigrafi = mikri_perigrafi.Substring(0, 25)
                    End If
                    gapfill(25, mikri_perigrafi.Length)
                    mikri_perigrafi = mikri_perigrafi & keno
                    lst.Add(mikri_perigrafi)

                    fpa = dt.Rows(i).Item(2)
                    gapfill(3, fpa.Length)
                    fpa = keno & fpa
                    lst.Add(fpa)

                    gapfill(8, 0)
                    lst.Add(keno)

                    lianiki = dt.Rows(i).Item(3)
                    lianiki = lianiki.Replace(",", "")
                    gapfill(10, lianiki.Length)
                    lianiki = lianiki & keno
                    lst.Add(lianiki)

                    gapfill(11, 0)
                    lst.Add(keno)

                    lst.Add("00001")

                    gapfill(31, 0)
                    lst.Add(keno)

                    zugistikos_kwdikos = dt.Rows(i).Item(4)
                    gapfill(5, zugistikos_kwdikos.Length)
                    zugistikos_kwdikos = zugistikos_kwdikos & keno
                    lst.Add(zugistikos_kwdikos)

                    barcode1 = dt.Rows(i).Item(5)
                    If barcode1.Length > 13 Or barcode1.Length < 7 Then
                        MessageBox.Show("Το barcode " & barcode1 & " δεν είναι έγκυρο και το είδος θα παραλειφθεί")
                        Continue For

                    End If
                    If barcode1.Length = 12 Or barcode1.Length = 7 Then
                        barcode1 = "0" & barcode1
                    End If
                    gapfill(13, barcode1.Length)
                    barcode1 = barcode1 & keno
                    lst.Add(barcode1)

                    mm1 = dt.Rows(i).Item(6)
                    If mm1.Length = 1 Then
                        mm1 = "0" & mm1
                    End If
                    lst.Add(mm1)

                    gapfill(5, 0)
                    lst.Add(keno)

                    gapfill(198, 0)
                    lst.Add(keno)

                    perigrafi = dt.Rows(i).Item(1)
                    gapfill(50, perigrafi.Length)
                    perigrafi = perigrafi & keno
                    lst.Add(perigrafi)

                    gapfill(18, 0)
                    lst.Add(keno)

                    lst.Add("  001")
                    lst.Add("0")

                    txt = lst(0) & " " & lst(1) & "   " & lst(2) & lst(3) & lst(4) & lst(5) & lst(6) & lst(7) & lst(8) & " " & lst(9) & " " & lst(10) & lst(11) & "1" & lst(12) & lst(13) & "0" & lst(14) & lst(15) & " " & lst(16)
                    sw.WriteLine(txt)
                Next
                runquery("update signet.app_remote set lastupdate=now()")
            End Using
            MessageBox.Show("Το αρχείο δημιουργήθηκε επιτυχώς!")
        Catch ex As Exception
            MessageBox.Show(ex.ToString)
        End Try

    End Sub
    Public Function returnsinglevaluequery(ByVal query As String) As Object
        Dim item As Object

        mysqlcon = New MySqlConnection
        'mysqlcon.ConnectionString = "server=localhost;userid=root;password=12345;database=sigmix"
        mysqlcon.ConnectionString = constr

        Try
            mysqlcon.Open()

            command = New MySqlCommand(query, mysqlcon)
            item = command.ExecuteScalar()

            mysqlcon.Close()
        Catch ex As Exception

            MessageBox.Show(ex.Message)

            Exit Function
        End Try
        If mysqlcon.State = ConnectionState.Open Then
            mysqlcon.Close()
        End If
        Return item
    End Function
    Public Sub runquery(ByVal query As String)
        queryerror = False
        mysqlcon = New MySqlConnection
        'mysqlcon.ConnectionString = "server=localhost;userid=root;password=12345;database=sigmix"
        mysqlcon.ConnectionString = constr

        Try
            mysqlcon.Open()
            trans = mysqlcon.BeginTransaction
            command = New MySqlCommand(query, mysqlcon)
            command.ExecuteNonQuery()
            'reader = command.ExecuteReader
            trans.Commit()
            mysqlcon.Close()
        Catch ex As Exception
            queryerror = True
            MessageBox.Show(ex.Message)
            trans.Rollback()

        End Try
        If mysqlcon.State = ConnectionState.Open Then
            mysqlcon.Close()
        End If

    End Sub
End Class
