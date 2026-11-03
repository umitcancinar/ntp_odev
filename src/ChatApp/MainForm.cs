using ChatApp.Core;
using System.Net.Sockets;

namespace ChatApp;

public sealed class MainForm : Form
{
    private readonly ChatServer server = new();
    private readonly ChatClient client = new();
    private readonly SemaphoreSlim serverGate = new(1, 1), clientGate = new(1, 1);
    private readonly NumericUpDown serverPort = Port("Sunucu portu"), clientPort = Port("Bağlanılacak port");
    private readonly CheckBox lan = new() { Text = "Yerel ağdan bağlantılara izin ver", AutoSize = true };
    private readonly TextBox host = new() { Text = "127.0.0.1", Width = 160, AccessibleName = "Sunucu adresi" };
    private readonly TextBox username = new() { Text = "Öğrenci", MaxLength = 32, Width = 160, AccessibleName = "Kullanıcı adı" };
    private readonly Button start = Button("Sunucuyu &başlat"), stop = Button("Sunucuyu &durdur");
    private readonly Button connect = Button("Bağ&lan"), disconnect = Button("Bağlantıyı &kes"), send = Button("&Gönder");
    private readonly Label serverStatus = Status("Sunucu kapalı."), clientStatus = Status("Bağlı değil.");
    private readonly RichTextBox serverLog = Log("Sunucu olayları"), conversation = Log("Zaman damgalı sohbet");
    private readonly ListView clients = new() { View = View.Details, FullRowSelect = true, Dock = DockStyle.Fill,
        AccessibleName = "Bağlı istemciler", HeaderStyle = ColumnHeaderStyle.Nonclickable };
    private readonly TextBox message = new() { Multiline = true, Dock = DockStyle.Fill, MaxLength = 2000,
        AcceptsReturn = true, ScrollBars = ScrollBars.Vertical, AccessibleName = "Gönderilecek mesaj" };
    private readonly Label counter = Status("0 / 2000 karakter • Ctrl+Enter ile gönder");
    private bool closing, closeReady;

    public MainForm()
    {
        Text = "YMH219 • TCP Sohbet";
        Font = new Font("Segoe UI", 10);
        AutoScaleMode = AutoScaleMode.Dpi;
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(1040, 800);
        MinimumSize = new Size(860, 680);
        var tabs = new TabControl { Dock = DockStyle.Fill, AccessibleName = "Sunucu ve istemci görünümleri" };
        tabs.TabPages.Add(ServerTab()); tabs.TabPages.Add(ClientTab());
        Controls.Add(tabs);
        stop.Enabled = disconnect.Enabled = send.Enabled = message.Enabled = false;
        start.Click += async (_, _) => await ServerActionAsync(StartServer);
        stop.Click += async (_, _) => await ServerActionAsync(async () => { await server.StopAsync(); serverStatus.Text = "Sunucu kapalı."; Append(serverLog, "Sunucu durduruldu."); });
        connect.Click += async (_, _) => await ClientActionAsync(async () =>
        {
            clientStatus.Text = "Bağlanıyor… (en fazla 6 saniye)";
            await client.ConnectAsync(host.Text.Trim(), (int)clientPort.Value, username.Text);
            clientStatus.Text = $"Bağlı: {username.Text.Trim()} • {host.Text}:{clientPort.Value}";
            message.Focus();
        });
        disconnect.Click += async (_, _) => await ClientActionAsync(async () => { await client.DisconnectAsync(); clientStatus.Text = "Bağlantı kesildi."; });
        send.Click += async (_, _) => await SendMessageAsync();
        message.KeyDown += async (_, e) =>
        {
            if (e.Control && e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; await SendMessageAsync(); }
        };
        message.TextChanged += (_, _) => counter.Text = $"{message.TextLength} / 2000 karakter • Ctrl+Enter ile gönder";
        server.MessageReceived += p => OnUi(() => Append(serverLog, Format(p)));
        server.ClientsChanged += () => OnUi(UpdateClients);
        server.Error += text => OnUi(() => Append(serverLog, text));
        client.MessageReceived += p => OnUi(() =>
        {
            if (p.Type == "clients")
                clientStatus.Text = $"Bağlı • Odadaki kullanıcılar ({p.Clients?.Length ?? 0}): {string.Join(", ", p.Clients?.Select(c => c.User) ?? [])}";
            else Append(conversation, Format(p));
        });
        client.Disconnected += text => OnUi(() => { clientStatus.Text = text; Append(conversation, text); UpdateClientControls(); });
        FormClosing += ClosingAsync;
    }

    private TabPage ServerTab()
    {
        var tab = new TabPage("Sunucu");
        var layout = CreateLayout(7);
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 160));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.Controls.Add(Heading("Sohbet sunucusu", "Önce sunucuyu açın; sonra bu veya başka bir pencereden istemci bağlayın."), 0, 0);
        layout.Controls.Add(Flow(new Label { Text = "Port", AutoSize = true, Padding = new Padding(0, 7, 0, 0) }, serverPort, lan, start, stop), 0, 1);
        layout.Controls.Add(serverStatus, 0, 2);
        layout.Controls.Add(Status("Bağlı istemciler • kullanıcı adı ve uzak adres"), 0, 3);
        clients.Columns.Add("Kullanıcı adı", 240); clients.Columns.Add("Adres : port", 360);
        layout.Controls.Add(clients, 0, 4);
        layout.Controls.Add(Status("Sunucu olayları ve mesajlar"), 0, 5);
        layout.Controls.Add(serverLog, 0, 6);
        tab.Controls.Add(layout); return tab;
    }

    private TabPage ClientTab()
    {
        var tab = new TabPage("İstemci"); var layout = CreateLayout(8);
        for (int i = 0; i < 4; i++) layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 85));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.Controls.Add(Heading("Sohbete katılın", "Aynı bilgisayar için 127.0.0.1 kullanın. İki kişi için uygulamayı iki kez açın."), 0, 0);
        layout.Controls.Add(Flow(Field("Sunucu adresi", host), Field("Port", clientPort), Field("Kullanıcı adı", username)), 0, 1);
        layout.Controls.Add(Flow(connect, disconnect), 0, 2);
        layout.Controls.Add(clientStatus, 0, 3);
        layout.Controls.Add(conversation, 0, 4);
        layout.Controls.Add(Status("Mesajınız"), 0, 5);
        layout.Controls.Add(message, 0, 6);
        var bottom = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, AutoSize = true };
        bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); bottom.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        bottom.Controls.Add(counter, 0, 0); bottom.Controls.Add(send, 1, 0);
        layout.Controls.Add(bottom, 0, 7); tab.Controls.Add(layout); return tab;
    }

    private Task StartServer()
    {
        server.Start((int)serverPort.Value, lan.Checked);
        serverStatus.Text = $"Sunucu açık • {(lan.Checked ? "Tüm IPv4 adresleri" : "Yalnızca bu bilgisayar")} : {server.Port}";
        Append(serverLog, "Sunucu başlatıldı."); return Task.CompletedTask;
    }

    private async Task ServerActionAsync(Func<Task> action)
    {
        if (closing || !await serverGate.WaitAsync(0)) return;
        start.Enabled = stop.Enabled = false;
        try { await action(); }
        catch (Exception ex) when (ConnectionError(ex)) { serverStatus.Text = $"İşlem yapılamadı: {Friendly(ex)}"; Append(serverLog, serverStatus.Text); }
        finally
        {
            start.Enabled = !closing && !server.IsRunning; stop.Enabled = !closing && server.IsRunning;
            serverPort.Enabled = lan.Enabled = !closing && !server.IsRunning;
            serverGate.Release();
        }
    }

    private async Task ClientActionAsync(Func<Task> action)
    {
        if (closing || !await clientGate.WaitAsync(0)) return;
        connect.Enabled = disconnect.Enabled = send.Enabled = false;
        try { await action(); }
        catch (Exception ex) when (ConnectionError(ex))
        { clientStatus.Text = $"İşlem yapılamadı: {Friendly(ex)}"; Append(conversation, clientStatus.Text); }
        finally { UpdateClientControls(); clientGate.Release(); }
    }

    private async Task SendMessageAsync() => await ClientActionAsync(async () =>
    {
        await client.SendAsync(message.Text);
        message.Clear(); message.Focus();
    });

    private void UpdateClientControls()
    {
        connect.Enabled = !closing && !client.IsConnected;
        disconnect.Enabled = send.Enabled = message.Enabled = !closing && client.IsConnected;
        host.Enabled = username.Enabled = clientPort.Enabled = !closing && !client.IsConnected;
    }

    private void UpdateClients()
    {
        clients.BeginUpdate(); clients.Items.Clear();
        foreach (var info in server.Clients) clients.Items.Add(new ListViewItem([info.User, info.Endpoint]));
        clients.EndUpdate();
    }

    private void OnUi(Action action)
    {
        if (closing || IsDisposed || !IsHandleCreated) return;
        try { BeginInvoke(() => { if (!closing && !IsDisposed) action(); }); }
        catch (InvalidOperationException) { /* Form handle may close between the check and BeginInvoke. */ }
    }

    private async void ClosingAsync(object? sender, FormClosingEventArgs e)
    {
        if (closeReady) return;
        e.Cancel = true;
        if (closing) return;
        closing = true; Enabled = false;
        clientStatus.Text = "Bağlantılar kapatılıyor…";
        await clientGate.WaitAsync(); await serverGate.WaitAsync();
        try { await client.DisposeAsync(); await server.DisposeAsync(); }
        finally { closeReady = true; Close(); }
    }

    private static bool ConnectionError(Exception ex) => ex is SocketException or IOException or InvalidDataException or
        ArgumentException or InvalidOperationException or OperationCanceledException or ObjectDisposedException;
    private static string Friendly(Exception ex) => ex switch
    {
        SocketException => "Adresi ve portu kontrol edin; sunucunun açık olduğundan emin olun. " + ex.Message,
        OperationCanceledException => "Bağlantı zaman aşımına uğradı veya iptal edildi.",
        _ => ex.Message
    };
    private static string Format(ChatPacket p) => $"[{(p.Timestamp ?? DateTimeOffset.Now).ToLocalTime():HH:mm:ss}] {p.User}: {p.Text}";
    private static void Append(RichTextBox log, string text)
    {
        if (log.Lines.Length >= 1000) log.Lines = log.Lines.Skip(200).ToArray();
        log.AppendText((text.Replace("\r", "").Replace("\n", " ↵ ")) + Environment.NewLine);
        log.SelectionStart = log.TextLength; log.ScrollToCaret();
    }
    private static NumericUpDown Port(string name) => new() { Minimum = 1, Maximum = 65535, Value = 5000, Width = 90, AccessibleName = name };
    private static Button Button(string text) => new() { Text = text, AutoSize = true, MinimumSize = new Size(100, 36), Padding = new Padding(6, 2, 6, 2) };
    private static Label Status(string text) => new() { Text = text, AutoSize = true, Dock = DockStyle.Fill, Padding = new Padding(0, 7, 0, 7) };
    private static RichTextBox Log(string name) => new() { ReadOnly = true, DetectUrls = false, Dock = DockStyle.Fill,
        BackColor = SystemColors.Window, AccessibleName = name, HideSelection = false };
    private static TableLayoutPanel CreateLayout(int rows) => new() { Dock = DockStyle.Fill, Padding = new Padding(18), ColumnCount = 1, RowCount = rows };
    private static FlowLayoutPanel Flow(params Control[] children)
    {
        var flow = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, WrapContents = true, Margin = new Padding(0, 4, 0, 4) };
        flow.Controls.AddRange(children); return flow;
    }
    private static Control Field(string label, Control input)
    {
        var panel = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.TopDown, WrapContents = false };
        panel.Controls.Add(new Label { Text = label, AutoSize = true }); panel.Controls.Add(input); return panel;
    }
    private static Control Heading(string title, string description)
    {
        var box = new TableLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, ColumnCount = 1, Margin = new Padding(0, 0, 0, 12) };
        box.Controls.Add(new Label { Text = title, Font = new Font("Segoe UI", 18, FontStyle.Bold), AutoSize = true });
        box.Controls.Add(Status(description)); return box;
    }
}
