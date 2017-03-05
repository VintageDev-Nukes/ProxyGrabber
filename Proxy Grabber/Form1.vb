Imports System.Collections
Imports System.Collections.Generic
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Net
Imports System.IO
Imports System.Threading
Imports System.Management

Public Enum labelType
    lblProxies
    lblEstado
End Enum

Public Enum stateType
    Scan
    Export
End Enum

Public Class Form1

    Public Event ServiceChanged As EventHandler
    Public Event ProxyListChanged As EventHandler

    Private _sServiceID, IndexPattern As Integer
    Private secondsToRead, secondsToExport As Double

    Private ReadOnly IPPattern As String = "(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)"
    Private ReadOnly PortPattern As String = "[0-9]{1,5}"
    Private ReadOnly ProxyPattern As String = "(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\:[0-9]{1,5}"
    Private ReadOnly defaultcmbText As String = "< ... >"
    Private ReadOnly AppData As String = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)

    Dim filePath As String

    Dim TimerStart As DateTime
    Dim TimeSpent As TimeSpan

    Dim client As WebClient = New WebClient

    Dim isDownloaded As Boolean

    Private ReadOnly Property ProxyList As ListBox.ObjectCollection
        Get
            Return lbProxies.Items
        End Get
    End Property

    Public Property selectedServiceID() As Integer
        Get
            Return cbServices.SelectedIndex
        End Get
        Set(value As Integer)
            _sServiceID = value
            RaiseEvent ServiceChanged(Me, Nothing)
        End Set
    End Property

    Public ReadOnly Property Url() As String
        Get
            Select Case Me.GetSelecetedServiceID
                Case 0 : Return "http://proxy-list.org/"
                Case 1 : Return "http://www.ip-adress.com/proxy_list/"
                Case 2 : Return "http://www.us-proxy.org/"
                Case 3 : Return "http://free-proxy-list.net/"
                Case 4 : Return "https://nordvpn.com/free-proxy-list/"
                Case 5 : Return "http://www.gatherproxy.com/"
                Case 6 : Return "http://proxylist.hidemyass.com/"
                Case 7 : Return "http://www.cool-proxy.net/proxies"
                Case 8 : Return "http://proxies.org/"
                Case Else
                    Return ""
            End Select
        End Get
    End Property

    Public ReadOnly Property ProxyCount() As Integer
        Get
            Return lbProxies.Items.Count
        End Get
    End Property

    Private Function GetSelecetedServiceID() As Integer
        With cbServices
            If .InvokeRequired Then
                Return CInt(.Invoke(New Func(Of Integer)(AddressOf GetSelecetedServiceID)))
            Else
                Return .SelectedIndex
            End If
        End With
    End Function

    Private Function GetBetween(ByVal Source As String, ByVal Str1 As String, ByVal Str2 As String, Optional ByVal Index As Integer = 0) As String
        Return Regex.Split(Regex.Split(Source, Str1)(Index + 1), Str2)(0)
    End Function

    Private Function GetBetweenAll(ByVal Source As String, ByVal Str1 As String, ByVal Str2 As String) As String()
        Dim Results, T As New List(Of String)
        T.AddRange(Regex.Split(Source, Str1))
        T.RemoveAt(0)
        For Each I As String In T
            Results.Add(Regex.Split(I, Str2)(0))
        Next
        Return Results.ToArray
    End Function

    Private Function GetMonthName(ByVal monthNum As Integer) As String
        Try
            Dim strDate As New DateTime(1, monthNum, 1)
            Dim English As New Globalization.CultureInfo("en-US")
            Return strDate.ToString("MMMM", English).ToLower
        Catch ex As Exception
            Return ""
        End Try
    End Function

    Public Function md5(ByVal input As String) As String
        Dim x As New System.Security.Cryptography.MD5CryptoServiceProvider()
        Dim bs As Byte() = System.Text.Encoding.UTF8.GetBytes(Input)
        bs = x.ComputeHash(bs)
        Dim s As New System.Text.StringBuilder()
        For Each b As Byte In bs
            s.Append(b.ToString("x2").ToLower())
        Next
        Dim password As String = s.ToString()
        Return password
    End Function

    Public Function base64_encode(ByVal input As String) As String
        Dim bytesToEncode As Byte()
        bytesToEncode = Encoding.UTF8.GetBytes(input)

        Dim encodedText As String
        encodedText = Convert.ToBase64String(bytesToEncode)

        Return (encodedText)
    End Function

    Public Function base64_decode(ByVal input As String) As String
        Dim decodedBytes As Byte()
        decodedBytes = Convert.FromBase64String(input)

        Dim decodedText As String
        decodedText = Encoding.UTF8.GetString(decodedBytes)

        Return (decodedText)
    End Function

    Function getIP(Optional ByVal webPath As String = "http://gimmeahit.x10host.com/c/getip.php") As String
        Return New System.Net.WebClient().DownloadString(webPath)
    End Function

    Function customGUID() As String
        Dim s As String = md5(base64_encode(getIP.Replace(".", ""))).ToUpper
        Dim guidText As String = s.Substring(0, 8) & "-" & s.Substring(8, 4) & "-" & s.Substring(12, 4) & "-" & s.Substring(16, 4) & "-" & s.Substring(20)
        Return guidText
    End Function

    Private Sub Export()

        TimerStart = Now

        ChangeText("Comenzando con la exportación...", labelType.lblEstado)

        File.AppendAllText(filePath, "[Lista de proxies]" & Environment.NewLine & "==================" & Environment.NewLine)

        For i As Integer = 0 To lbProxies.Items.Count - 1
            UpdateInfo(stateType.Export)
            Dim proxy As String = lbProxies.Items(i).ToString
            File.AppendAllText(filePath, Environment.NewLine & proxy)
        Next

        TimeSpent = Now.Subtract(TimerStart)
        secondsToExport = TimeSpent.TotalSeconds

        Dim iSpan As TimeSpan = TimeSpan.FromSeconds(secondsToRead)
        Dim iSpan1 As TimeSpan = TimeSpan.FromSeconds(secondsToExport)

        File.AppendAllText(filePath, String.Format(Environment.NewLine & Environment.NewLine & "[INFO GENERAL]" &
                                                   Environment.NewLine & "Total de proxies escaneadas: {0}" &
                                                   Environment.NewLine & "Tiempo tomado en leer: {1}" &
                                                   Environment.NewLine & "Tiempo medio por proxy en leer: {2} proxy/s" &
                                                   Environment.NewLine & "Tiempo tomado en exportar: {3}" &
                                                   Environment.NewLine & "Tiempo medio por proxy en exportar: {4} proxy/s" &
                                                   Environment.NewLine & Environment.NewLine & "==================" & Environment.NewLine & Environment.NewLine & "Esta lista fue generada con Proxy Cheker." & Environment.NewLine & "Una aplicación para comprobar si las proxies están disponibles." &
                                                   Environment.NewLine & "Ikillnukes ©  {5}",
                                                   ProxyList.Count,
                                                   If(iSpan.Days <> 0, iSpan.Days.ToString.PadLeft(2, "0"c) & " d ", "") &
                        If(iSpan.Hours <> 0, iSpan.Hours.ToString.PadLeft(2, "0"c) & " h ", "") &
                        If(iSpan.Minutes <> 0, iSpan.Minutes.ToString.PadLeft(2, "0"c) & " m ", "") &
                        If(iSpan.Seconds <> 0, iSpan.Seconds.ToString.PadLeft(2, "0"c) & " s", ""),
                                                   (ProxyList.Count / secondsToRead).ToString("F3", New Globalization.CultureInfo("en-US")),
                                                   If(iSpan1.Days <> 0, iSpan1.Days.ToString.PadLeft(2, "0"c) & " d ", "") &
                        If(iSpan1.Hours <> 0, iSpan1.Hours.ToString.PadLeft(2, "0"c) & " h ", "") &
                        If(iSpan1.Minutes <> 0, iSpan1.Minutes.ToString.PadLeft(2, "0"c) & " m ", "") &
                        If(iSpan1.Seconds <> 0, iSpan1.Seconds.ToString.PadLeft(2, "0"c) & " s", ""),
                                                   (ProxyList.Count / secondsToExport).ToString("F3", New Globalization.CultureInfo("en-US")),
                                                   Date.Today.Year))

        IndexPattern = 0

        ChangeText("¡Exportación terminada con éxito, gracias por confiar en nosotros! :D", labelType.lblEstado)

        Process.Start(filePath)

    End Sub

    Private Sub RemoveDupes()
        With lbProxies
            Dim items(.Items.Count - 1) As Object
            If .InvokeRequired Then
                .Invoke(Sub() .Items.CopyTo(items, 0))
                .Invoke(Sub() .Items.Clear())
                .Invoke(Sub() .Items.AddRange(items.AsEnumerable().Distinct().ToArray()))
            Else
                .Items.CopyTo(items, 0)
                .Items.Clear()
                .Items.AddRange(items.AsEnumerable().Distinct().ToArray())
            End If
        End With
        RaiseEvent ProxyListChanged(Me, Nothing)
    End Sub

    Private Sub ExitApp() Handles MyBase.FormClosed
        If System.Diagnostics.Process.GetCurrentProcess().Threads.Count > 0 Then
            For Each t As System.Diagnostics.ProcessThread In System.Diagnostics.Process.GetCurrentProcess().Threads
                t.Dispose()
            Next t
        End If
        Application.Exit()
    End Sub

    Private Sub UpdateInfo(Optional ByVal type As stateType = stateType.Scan)
        Threading.Interlocked.Increment(IndexPattern)
        ChangeText(String.Format("{0} proxies {1}...", IndexPattern, If(type = stateType.Scan, "obtenidas", "guardadas")), labelType.lblEstado)
    End Sub

    Private Sub Scan()

        Dim r As HttpWebRequest
        Dim re As HttpWebResponse
        Dim src As String = ""

        If Me.GetSelecetedServiceID <> 9 Then

            'Read all the source code

            TimerStart = Now

            ChangeText("Obteniendo source code...", labelType.lblEstado)

            r = HttpWebRequest.Create(Me.Url)
            r.UserAgent = "Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/30.0.1599.69 Safari/537.36"
            r.KeepAlive = True
            re = r.GetResponse() 'Make a double request with the cookies reveived in the response
            r = HttpWebRequest.Create(Me.Url)
            r.UserAgent = "Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/30.0.1599.69 Safari/537.36"
            r.KeepAlive = True
            r.CookieContainer = New CookieContainer()
            r.CookieContainer.Add(re.Cookies)
            Using source As New StreamReader(re.GetResponseStream())
                src = source.ReadToEnd() 'Source code
            End Using

            'Explode source code

            ChangeText("Obteniendo proxies...", labelType.lblEstado)

        End If

        Select Case Me.GetSelecetedServiceID

            Case 0 'Proxy List

                Dim pagesPattern As String = "<a class=""item"""
                Dim pagesMatches As MatchCollection = Regex.Matches(src, pagesPattern)
                Dim pagesNumber As Integer = pagesMatches.Count
                Dim completeSrc As String = src

                For i As Integer = 2 To pagesNumber

                    ChangeText(String.Format("Leyendo {0} de {1} páginas...", i, pagesNumber), labelType.lblEstado)

                    r = HttpWebRequest.Create(Me.Url & "?p=" & i)
                    r.UserAgent = "Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/30.0.1599.69 Safari/537.36"
                    r.KeepAlive = True
                    re = r.GetResponse() 'Make a double request with the cookies reveived in the response
                    r = HttpWebRequest.Create(Me.Url & "?p=" & i)
                    r.UserAgent = "Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/30.0.1599.69 Safari/537.36"
                    r.KeepAlive = True
                    r.CookieContainer = New CookieContainer()
                    r.CookieContainer.Add(re.Cookies)
                    Using source As New StreamReader(re.GetResponseStream())
                        src = source.ReadToEnd() 'Source code
                    End Using

                    completeSrc &= Environment.NewLine & Environment.NewLine & src

                Next

                ChangeText("Obteniendo proxies...", labelType.lblEstado)

                Dim proxies As Match = Regex.Match(completeSrc, ProxyPattern)

                Do While proxies.Success

                    UpdateInfo()

                    If lbProxies.InvokeRequired Then lbProxies.Invoke(Sub() lbProxies.Items.Add(proxies.Value)) Else lbProxies.Items.Add(proxies.Value)

                    proxies = proxies.NextMatch()

                    RaiseEvent ProxyListChanged(Me, Nothing)

                Loop

            Case 1 'IP Adress

                Dim proxies As Match = Regex.Match(src, ProxyPattern)

                Do While proxies.Success

                    UpdateInfo()

                    If lbProxies.InvokeRequired Then lbProxies.Invoke(Sub() lbProxies.Items.Add(proxies.Value)) Else lbProxies.Items.Add(proxies.Value)

                    proxies = proxies.NextMatch()

                    RaiseEvent ProxyListChanged(Me, Nothing)

                Loop

            Case 2, 3 'Proxy US & Free Proxy List
                Dim rows As String() = GetBetweenAll(src, "<tr>", "</tr>")
                Dim tds As New List(Of String)
                'Dim dones As New List(Of String)
                For Each s As String In rows
                    If (Not s = rows(0) And s.Contains("<td>") And s.Contains("</td>")) Then
                        Dim td As String() = GetBetweenAll(s, "<td>", "</td>")
                        Dim ip As String = td(0)
                        Dim port As String = td(1)
                        Dim rawProxy As String = ip & ":" & port
                        Dim match As Match = Regex.Match(rawProxy, ProxyPattern)
                        If match.Success And Regex.IsMatch(rawProxy, ProxyPattern) Then
                            UpdateInfo()
                            If lblProxies.InvokeRequired Then
                                lbProxies.Invoke(Sub() lbProxies.Items.Add(rawProxy))
                            Else
                                lbProxies.Items.Add(rawProxy)
                            End If
                            RaiseEvent ProxyListChanged(Me, Nothing)
                        End If
                    End If
                Next

            Case 4 'Nord VPN

                Dim lastPagePattern As String = "<a href=""\?&amp;page=\d+"" "
                Dim lastPageMatch As Match = Regex.Match(src, lastPagePattern)
                Dim lastPage As Integer = 0
                Dim completeSrc As String = src

                If lastPageMatch.Success Then

                    Dim getNumber As Match = Regex.Match(lastPageMatch.Value, "\d+")

                    If getNumber.Success Then

                        lastPage = CInt(getNumber.Value)

                    End If

                End If

                For i As Integer = 2 To lastPage

                    ChangeText(String.Format("Leyendo {0} de {1} páginas...", i, lastPage), labelType.lblEstado)

                    r = HttpWebRequest.Create(Me.Url & i)
                    r.UserAgent = "Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/30.0.1599.69 Safari/537.36"
                    r.KeepAlive = True
                    re = r.GetResponse() 'Make a double request with the cookies reveived in the response
                    r = HttpWebRequest.Create(Me.Url & i)
                    r.UserAgent = "Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/30.0.1599.69 Safari/537.36"
                    r.KeepAlive = True
                    r.CookieContainer = New CookieContainer()
                    r.CookieContainer.Add(re.Cookies)
                    Using source As New StreamReader(re.GetResponseStream())
                        src = source.ReadToEnd() 'Source code
                    End Using

                    completeSrc &= Environment.NewLine & Environment.NewLine & src

                Next

                ChangeText("Obteniendo proxies...", labelType.lblEstado)

                Dim newHtml As String = Regex.Replace(completeSrc, "</td><td>", ":")

                Dim proxies As Match = Regex.Match(newHtml, ProxyPattern)

                Do While proxies.Success

                    UpdateInfo()

                    If lbProxies.InvokeRequired Then lbProxies.Invoke(Sub() lbProxies.Items.Add(proxies.Value)) Else lbProxies.Items.Add(proxies.Value)

                    proxies = proxies.NextMatch()

                    RaiseEvent ProxyListChanged(Me, Nothing)

                Loop

            Case 5 'Gather Proxy

                Dim cleanPattern As String = "(PROXY_IP"":""|""|PROXY_PORT)"
                Dim getProxiesPattern As String = "(""PROXY_IP"":""(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)""|""PROXY_PORT"":""\d+"")"
                Dim getProxiesMatch As Match = Regex.Match(src, getProxiesPattern)
                Dim proxyTogether As String = ""

                Do While getProxiesMatch.Success

                    proxyTogether &= getProxiesMatch.Value

                    getProxiesMatch = getProxiesMatch.NextMatch()

                Loop

                Dim newIps1 As String = Regex.Replace(proxyTogether, """""", Environment.NewLine)

                Dim newIps2 As String = Regex.Replace(newIps1, cleanPattern, "")

                Dim newIps3 As String = Regex.Replace(newIps2, "\r\n:", ":")

                Dim proxies As Match = Regex.Match(newIps3, ProxyPattern)

                Do While proxies.Success

                    UpdateInfo()

                    If lbProxies.InvokeRequired Then lbProxies.Invoke(Sub() lbProxies.Items.Add(proxies.Value)) Else lbProxies.Items.Add(proxies.Value)

                    proxies = proxies.NextMatch()

                    RaiseEvent ProxyListChanged(Me, Nothing)

                Loop

            Case 6 'Hide My Ass
                Dim pagesCountPattern As String = "<a href=""/\d+"">\d+</a>"
                Dim deleteHtml As String = "</?\w+.*?>"
                Dim stylePattern As String = "<style.+?</style>"
                Dim classesPattern As String = "(?<=[\.])\S{4}(?=[\{])"
                Dim nClassesPattern As String = "(?<=[\.])\S{4}\{display:none"
                Dim getDivSpanPattern As String = "<(span|div) style=""{0}"">.+?</(span|div)>"
                Dim getDivSpanClassesPattern As String = "<(span|div) class=""{0}"">.+?</(span|div)>"
                Dim portPattern As String = "</td>.+\n.+<td>\n"
                Dim blockPattern As String = "<tr class=""(altshade|)""  rel=""\d+"">.+<td style=""text-align:left"""
                Dim match As MatchCollection = Regex.Matches(src, pagesCountPattern)
                Dim pagesNumber As Integer = match.Count + 1
                Dim completeSrc As String = src
                Dim StylesBlock As String = ""
                Dim inlineStylesStr As String = ""
                Dim noneStylesStr As String = ""
                Dim rawHtmlStr As String = ""
                Dim InlineList As List(Of String) = New List(Of String)
                Dim NoneList As List(Of String) = New List(Of String)
                Dim IPBlock As String = ""

                For i As Integer = 2 To pagesNumber

                    ChangeText(String.Format("Leyendo {0} de {1} páginas...", i, pagesNumber), labelType.lblEstado)

                    r = HttpWebRequest.Create(Me.Url & i)
                    r.UserAgent = "Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/30.0.1599.69 Safari/537.36"
                    r.KeepAlive = True
                    re = r.GetResponse() 'Make a double request with the cookies reveived in the response
                    r = HttpWebRequest.Create(Me.Url & i)
                    r.UserAgent = "Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/30.0.1599.69 Safari/537.36"
                    r.KeepAlive = True
                    r.CookieContainer = New CookieContainer()
                    r.CookieContainer.Add(re.Cookies)
                    Using source As New StreamReader(re.GetResponseStream())
                        src = source.ReadToEnd() 'Source code
                    End Using

                    completeSrc &= Environment.NewLine & Environment.NewLine & src

                Next

                ChangeText("Obteniendo proxies...", labelType.lblEstado)

                Dim styleMatches As Match = Regex.Match(completeSrc, stylePattern, RegexOptions.Singleline)

                Do While styleMatches.Success

                    StylesBlock &= styleMatches.Value

                    styleMatches = styleMatches.NextMatch()

                Loop

                Dim noneMatches As Match = Regex.Match(StylesBlock, nClassesPattern, RegexOptions.Singleline)

                Do While noneMatches.Success

                    NoneList.Add(noneMatches.Value.Replace("{display:none", ""))

                    noneMatches = noneMatches.NextMatch()

                Loop

                Dim blockMatches As Match = Regex.Match(completeSrc, blockPattern, RegexOptions.Singleline)

                Do While blockMatches.Success

                    IPBlock &= blockMatches.Value

                    blockMatches = blockMatches.NextMatch()

                Loop

                Dim noneClassesMatches As Match = Regex.Match(StylesBlock, nClassesPattern, RegexOptions.Singleline)

                Do While noneClassesMatches.Success

                    noneStylesStr &= noneClassesMatches.Value

                    noneClassesMatches = noneClassesMatches.NextMatch()

                Loop

                Dim deleteNoneStyles As String = Regex.Replace(IPBlock, String.Format(getDivSpanPattern, "display:none"), "", RegexOptions.Singleline)

                Dim rawHtml As String = deleteNoneStyles

                For Each cssClass As String In NoneList

                    Dim newHtml As String = Regex.Replace(rawHtml, String.Format(getDivSpanClassesPattern, cssClass), "", RegexOptions.Singleline)

                    rawHtml = newHtml

                Next

                Dim portDelete As String = Regex.Replace(rawHtml, portPattern, ":")

                Dim finalText As String = Regex.Replace(portDelete, deleteHtml, "", RegexOptions.Singleline)

                Dim proxies As Match = Regex.Match(finalText, ProxyPattern)

                Do While proxies.Success

                    UpdateInfo()

                    If lbProxies.InvokeRequired Then lbProxies.Invoke(Sub() lbProxies.Items.Add(proxies.Value)) Else lbProxies.Items.Add(proxies.Value)

                    proxies = proxies.NextMatch()

                    RaiseEvent ProxyListChanged(Me, Nothing)

                Loop

            Case 7

                Dim deleteHtml As String = "</?\w+.*?>"
                Dim deleteNoneDivs As String = "<span style=""display:none"">.+?</span>"
                Dim lastPagePattern As String = "\.\.\..+?&nbsp;"
                Dim portPattern As String = "\n</td>\n.+<td>"
                Dim lastPageMatch As Match = Regex.Match(src, lastPagePattern)
                Dim lastPage As Integer = 0
                Dim completeSrc As String = src

                If lastPageMatch.Success Then

                    Dim getNumber As Match = Regex.Match(lastPageMatch.Value, "\d+")

                    If getNumber.Success Then

                        lastPage = CInt(getNumber.Value)

                    End If

                End If

                For i As Integer = 2 To lastPage

                    ChangeText(String.Format("Leyendo {0} de {1} páginas...", i, lastPage), labelType.lblEstado)

                    r = HttpWebRequest.Create(Me.Url & "?page=" & i)
                    r.UserAgent = "Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/30.0.1599.69 Safari/537.36"
                    r.KeepAlive = True
                    re = r.GetResponse() 'Make a double request with the cookies reveived in the response
                    r = HttpWebRequest.Create(Me.Url & "?page=" & i)
                    r.UserAgent = "Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/30.0.1599.69 Safari/537.36"
                    r.KeepAlive = True
                    r.CookieContainer = New CookieContainer()
                    r.CookieContainer.Add(re.Cookies)
                    Using source As New StreamReader(re.GetResponseStream())
                        src = source.ReadToEnd() 'Source code
                    End Using

                    completeSrc &= Environment.NewLine & Environment.NewLine & src

                Next

                Dim newHtml1 As String = Regex.Replace(completeSrc, portPattern, ":")

                Dim newHtml2 As String = Regex.Replace(newHtml1, deleteNoneDivs, "")

                Dim newHtml3 As String = Regex.Replace(newHtml2, deleteHtml, "")

                Dim newHtml4 As String = Regex.Replace(newHtml3, "\n:", ":")

                Dim proxies As Match = Regex.Match(newHtml4, ProxyPattern)

                Do While proxies.Success

                    UpdateInfo()

                    If lbProxies.InvokeRequired Then lbProxies.Invoke(Sub() lbProxies.Items.Add(proxies.Value)) Else lbProxies.Items.Add(proxies.Value)

                    proxies = proxies.NextMatch()

                    RaiseEvent ProxyListChanged(Me, Nothing)

                Loop

            Case 8

                Dim startYear As Date = DateTime.Parse(String.Format("01-01-{0}", Date.Now.Year))
                Dim firstDate As Date = DateTime.Parse(String.Format("01-02-{0}", Date.Now.Year))
                Dim daysFromToday As Long = DateDiff(DateInterval.Day, firstDate, Now)
                Dim daysFromStart As Long = DateDiff(DateInterval.Day, startYear, firstDate)

                For i As Integer = daysFromStart To daysFromToday

                    Dim numberOfDays As Integer = i
                    Dim [date] As New DateTime(New TimeSpan(numberOfDays - 1, 0, 0, 0).Ticks)
                    Dim dateUntil As String = String.Format("{0}-{1}-{2}", GetMonthName([date].Month), [date].Day, ([date].Year - 1 + Date.Now.Year))

                    ChangeText(String.Format("Leyendo {0} de {1} entradas...", i - daysFromStart, daysFromToday - daysFromStart), labelType.lblEstado)

                    Try
                        r = HttpWebRequest.Create(Me.Url & dateUntil)
                        r.UserAgent = "Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/30.0.1599.69 Safari/537.36"
                        r.KeepAlive = True
                        re = r.GetResponse() 'Make a double request with the cookies reveived in the response
                        r = HttpWebRequest.Create(Me.Url & dateUntil)
                        r.UserAgent = "Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/30.0.1599.69 Safari/537.36"
                        r.KeepAlive = True
                        r.CookieContainer = New CookieContainer()
                        r.CookieContainer.Add(re.Cookies)
                        Using source As New StreamReader(re.GetResponseStream())
                            src = source.ReadToEnd() 'Source code
                        End Using
                    Catch ex As Exception
                    End Try

                    Dim proxies As Match = Regex.Match(src, ProxyPattern)

                    Do While proxies.Success

                        UpdateInfo()

                        If lbProxies.InvokeRequired Then lbProxies.Invoke(Sub() lbProxies.Items.Add(proxies.Value)) Else lbProxies.Items.Add(proxies.Value)

                        proxies = proxies.NextMatch()

                        RaiseEvent ProxyListChanged(Me, Nothing)

                    Loop

                Next

            Case 9

                For i As Integer = 0 To 8
                    If cbServices.InvokeRequired Then cbServices.Invoke(Sub() cbServices.SelectedIndex = i) Else cbServices.SelectedIndex = i
                    Scan()
                Next

            Case Else

                Exit Sub

        End Select

        RemoveDupes()

        TimeSpent = Now.Subtract(TimerStart)
        secondsToRead = TimeSpent.TotalSeconds

        ChangeText(String.Format("¡Tarea acabada! {0} proxies obtenidas...", IndexPattern), labelType.lblEstado)
        IndexPattern = 0

        If btnExport.InvokeRequired Then btnExport.Invoke(Sub() btnExport.Enabled = True) Else btnExport.Enabled = True
        If btnCheck.InvokeRequired Then btnCheck.Invoke(Sub() btnCheck.Enabled = True) Else btnCheck.Enabled = True

    End Sub

    Private Sub ChangeText(ByVal text As String, Optional ByVal lblType As labelType = labelType.lblProxies)
        If lblType = labelType.lblProxies Then
            If Not String.IsNullOrWhiteSpace(text) Then
                If lblProxies.InvokeRequired Then lblProxies.Invoke(Sub() lblProxies.Text = "Lista de proxies obtenidas " & text) Else lblProxies.Text = "Lista de proxies obtenidas " & text
            Else
                If lblProxies.InvokeRequired Then lblProxies.Invoke(Sub() lblProxies.Text = "Lista de proxies obtenidas") Else lblProxies.Text = "Lista de proxies obtenidas"
            End If
        ElseIf lblType = labelType.lblEstado Then
            If Not String.IsNullOrWhiteSpace(text) Then
                If lblEstado.InvokeRequired Then lblEstado.Invoke(Sub() lblEstado.Text = text) Else lblEstado.Text = text
            End If
        End If
    End Sub

    Private Sub Download(ByVal fileDownload As String, ByVal filePath As String)
        AddHandler client.DownloadProgressChanged, AddressOf client_ProgressChanged
        AddHandler client.DownloadFileCompleted, AddressOf client_DownloadCompleted
        client.DownloadFileAsync(New Uri(fileDownload), filePath)
    End Sub

    Private Sub Check()

        ChangeText("Haciendo unas pequeñas comprobaciones...", labelType.lblEstado)

        Dim fileCheck As String = AppData & "\Proxy Checker\ProxyChecker.exe"
        Dim fileDownload As String = "http://www.dropbox.com/s/gwe1x39252p67kf/ProxyChecker.exe?dl=1"
        Dim proxyFile As String = String.Format(AppData & "\Proxy Checker\Lista de Proxies del {0}.txt", Date.Now.ToString("d-M-yyyy HH mm"))

        If Not Directory.Exists(AppData & "\Proxy Checker\") Then
            Directory.CreateDirectory(AppData & "\Proxy Checker\")
        End If

        If Not File.Exists(fileCheck) Then
            Download(fileDownload, fileCheck)
        ElseIf isDownloaded Or File.Exists(fileCheck) Then

            ChangeText("Guardando archivo de proxies...", labelType.lblEstado)

            For Each proxy As String In ProxyList
                File.AppendAllText(proxyFile, proxy & Environment.NewLine)
            Next

            ChangeText("Abriendo ""Proxy Checker""...", labelType.lblEstado)

            Process.Start(fileCheck, String.Format("""{0}""", proxyFile))

            isDownloaded = False

        End If

    End Sub

    Private Shadows Sub Load() Handles MyBase.Load

        'Add all the services to the combobox

        cbServices.Text = defaultcmbText
        cbServices.Items.Add(New Services(0, "Proxy List"))
        cbServices.Items.Add(New Services(1, "IP Adress"))
        cbServices.Items.Add(New Services(2, "US Proxy"))
        cbServices.Items.Add(New Services(3, "Free Proxy List"))
        cbServices.Items.Add(New Services(4, "Nord VPN"))
        cbServices.Items.Add(New Services(5, "Gather Proxy"))
        cbServices.Items.Add(New Services(6, "Hide My Ass"))
        cbServices.Items.Add(New Services(7, "Cool Proxy"))
        cbServices.Items.Add(New Services(8, "Proxies"))
        cbServices.Items.Add(New Services(9, "ALL"))

        btnExport.Enabled = False
        btnCheck.Enabled = False

        SaveFileDialog1.Filter = "Text File|*.txt"

        ChangeText("Preparado para comenzar...", labelType.lblEstado)

    End Sub

    Private Sub client_ProgressChanged(ByVal sender As Object, ByVal e As DownloadProgressChangedEventArgs)

        Dim bytesIn As Double = Double.Parse(e.BytesReceived.ToString())
        Dim totalBytes As Double = Double.Parse(e.TotalBytesToReceive.ToString())
        Dim percentage As Double = bytesIn / totalBytes * 100

        ChangeText(String.Format("Descargando ""Proxy Cheker""... ({0} de {1} KB recibidos) [{2}%]", bytesIn / 1024, totalBytes / 1024, percentage), labelType.lblEstado)

    End Sub

    Private Sub client_DownloadCompleted(ByVal sender As Object, ByVal e As System.ComponentModel.AsyncCompletedEventArgs)
        isDownloaded = True
        Check()
    End Sub

    Private Sub btnGo_Click(sender As Object, e As EventArgs) Handles btnGo.Click
        Dim thScan As New Thread(AddressOf Scan)
        thScan.Start()
    End Sub

    Private Sub cbServices_TextChanged(sender As Object, e As EventArgs) Handles cbServices.TextChanged
        btnGo.Enabled = cbServices.Text <> defaultcmbText
    End Sub

    Private Sub ChangedList() Handles Me.ProxyListChanged
        ChangeText(String.Format("({0})", Me.ProxyCount))
    End Sub

    Private Sub btnExport_Click(sender As Object, e As EventArgs) Handles btnExport.Click
        SaveFileDialog1.ShowDialog()
    End Sub

    Private Sub Export_OK(sender As Object, e As System.ComponentModel.CancelEventArgs) Handles SaveFileDialog1.FileOk
        filePath = SaveFileDialog1.FileName
        Dim thExp As New Thread(AddressOf Export)
        thExp.Start()
    End Sub

    Private Sub btnCheck_Click(sender As Object, e As EventArgs) Handles btnCheck.Click
        Check()
    End Sub

End Class

Public Class Services
    Public ServiceID As Integer
    Public ServiceName As String
    Public Sub New(v As Integer, n As String)
        ServiceID = v
        ServiceName = n
    End Sub
    Public Overrides Function ToString() As String
        Return ServiceName
    End Function
End Class
