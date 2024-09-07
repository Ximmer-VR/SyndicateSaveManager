using System.Diagnostics;
using System.Text.Json;
using System.Windows.Forms;

namespace SyndicateSaveManager;

public partial class MainForm : Form
{
    private FileSystemWatcher _fileWatcher;
    private string _logFilePath;
    private string _jsonSaveFile = "saveStates.json";
    private long _mostRecentTimestamp = 0;

    public MainForm()
    {
        InitializeComponent();

        var appData = LoadDataFromJson();
        PopulateTreeView(appData.SaveStates);
        StartLogFileWatcher(appData.Settings.LogFilePath);
    }

    private void UpdateTreeViewSafe(Action action)
    {
        if (treeViewSaveStates.InvokeRequired)
        {
            treeViewSaveStates.Invoke(action);
        }
        else
        {
            action();
        }
    }

    private void PopulateTreeView(Dictionary<string, List<SaveState>> saveStates)
    {
        UpdateTreeViewSafe(() =>
        {
            treeViewSaveStates.Nodes.Clear();

            // Sort dates in descending order
            var sortedDates = saveStates.Keys.OrderByDescending(date => DateTime.Parse(date)).ToList();

            foreach (var date in sortedDates)
            {
                TreeNode dateNode = new TreeNode(date);

                // Sort save states by timestamp in descending order
                var sortedSaveStates = saveStates[date]
                    .OrderByDescending(ss => ss.Timestamp)
                    .ToList();

                foreach (var saveState in sortedSaveStates)
                {
                    DateTime utcDateTime = DateTimeOffset.FromUnixTimeSeconds(saveState.Timestamp).UtcDateTime;
                    TreeNode saveNode = new TreeNode(utcDateTime.ToLongTimeString())
                    {
                        Tag = saveState
                    };
                    dateNode.Nodes.Add(saveNode);
                }

                treeViewSaveStates.Nodes.Add(dateNode);
            }
        });
    }

    private AppData LoadDataFromJson()
    {
        if (File.Exists(_jsonSaveFile))
        {
            string json = File.ReadAllText(_jsonSaveFile);
            var data = JsonSerializer.Deserialize<AppData>(json);

            // Set log file path from settings, or use default if not present
            if (string.IsNullOrEmpty(data.Settings?.LogFilePath))
            {
                data.Settings = new Settings { LogFilePath = GetDefaultLogFilePath() };
            }
            _logFilePath = data.Settings.LogFilePath;

            // Set the most recent timestamp from the save states
            if (data.SaveStates != null && data.SaveStates.Count > 0)
            {
                _mostRecentTimestamp = data.SaveStates
                    .SelectMany(s => s.Value)
                    .Max(s => s.Timestamp);
            }

            return data;
        }

        // If no file exists, return with default log path and empty save states
        return new AppData
        {
            Settings = new Settings { LogFilePath = GetDefaultLogFilePath() },
            SaveStates = new Dictionary<string, List<SaveState>>()
        };
    }

    private string GetDefaultLogFilePath()
    {
        string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return Path.Combine(userProfile, "AppData", "LocalLow", "VRChat", "VRChat");
    }

    private void StartLogFileWatcher(string logFilePath)
    {
        _fileWatcher = new FileSystemWatcher(logFilePath, "*.txt"); // Only watch .txt files
        _fileWatcher.Changed += OnLogFileChanged;
        _fileWatcher.EnableRaisingEvents = true;
    }

    private void OnLogFileChanged(object sender, FileSystemEventArgs e)
    {
        try
        {
            // Open the file with FileShare.ReadWrite to allow access while another process has the file open
            using (FileStream fs = new FileStream(e.FullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (StreamReader sr = new StreamReader(fs))
            {
                string content = sr.ReadToEnd();
                ProcessLogFileContent(content);
            }
        }
        catch (IOException ex)
        {
            // Handle any exceptions that may occur, such as file locks or access issues
            Debug.WriteLine($"Error reading log file: {ex.Message}");
        }
    }

    private void ProcessLogFileContent(string content)
    {
        Debug.WriteLine($"ProcessLogFileContent");

        // Split the content into lines
        var lines = content.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var lineFull in lines)
        {
            string line = lineFull.Trim();

            // Check if the line contains "[Syndicate-Save]"
            if (line.Contains("[Syndicate-Save]"))
            {
                Debug.WriteLine($"line: {line}");

                try
                {
                    // Example log line format:
                    // 2024.09.06 09:53:14 Log        -  [Syndicate-Save] <created_utc_long> <base64_data>
                    var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    // Extract the date and time
                    string datePart = $"{parts[0]} {parts[1]}";
                    DateTime logDateTime = DateTime.ParseExact(datePart, "yyyy.MM.dd HH:mm:ss", null);

                    DateTimeOffset logTimestampOffset = new DateTimeOffset(logDateTime, TimeSpan.Zero);
                    long logTimestamp = logTimestampOffset.ToUnixTimeSeconds();

                    // Extract the Unix timestamp and Base64 save data
                    long createdTimestamp = long.Parse(parts[5]); // parts[5] is the Unix timestamp of the log
                    string base64SaveData = parts[6];         // parts[6] is the Base64 data

                    // Update the most recent log timestamp
                    if (logTimestamp > _mostRecentTimestamp)
                    {
                        Debug.WriteLine($"new");

                        _mostRecentTimestamp = logTimestamp;
                        StoreSaveState(logTimestamp, createdTimestamp, base64SaveData);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to process log line: {ex.Message}");
                }
            }
        }
    }

    private void StoreSaveState(long timestamp, long created, string base64Save)
    {
        string date = DateTimeOffset.FromUnixTimeSeconds(timestamp).ToString("yyyy-MM-dd");

        // Load existing JSON or create new with default log file path
        var data = LoadDataFromJson();

        // Check if the date already exists
        if (!data.SaveStates.ContainsKey(date))
        {
            data.SaveStates[date] = new List<SaveState>();
        }

        // Check for duplicate timestamps
        if (!data.SaveStates[date].Any(ss => ss.Timestamp == timestamp))
        {
            data.SaveStates[date].Add(new SaveState { Created = created, Timestamp = timestamp, Base64 = base64Save });

            // Create JsonSerializerOptions with indentation
            var options = new JsonSerializerOptions
            {
                WriteIndented = true // Enable indentation
            };

            // Write back to the JSON file with indentation
            File.WriteAllText(_jsonSaveFile, JsonSerializer.Serialize(data, options));

            // Update the TreeView
            PopulateTreeView(data.SaveStates);
        }
        else
        {
            Debug.WriteLine($"Duplicate save state ignored: Timestamp {timestamp}");
        }
    }

    private void TreeViewSaveStates_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
    {
        if (e.Node.Tag is SaveState saveState)
        {
            Clipboard.SetText(saveState.Base64); // Copy the base64 data to the clipboard
            UpdateStatusStrip("Copied to clipboard");
        }
    }

    private void UpdateLogFilePath(string newPath)
    {
        _logFilePath = newPath;

        // Load current data, update settings, and save
        var data = LoadDataFromJson();
        data.Settings.LogFilePath = _logFilePath;
        File.WriteAllText(_jsonSaveFile, JsonSerializer.Serialize(data));
    }

    private void exitToolStripMenuItem_Click(object sender, EventArgs e)
    {
        Close();
    }

    private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
    {
        var settingsDialog = new SettingsDialog(_logFilePath);

        if (settingsDialog.ShowDialog() == DialogResult.OK)
        {
            _logFilePath = settingsDialog.LogFilePath;
            UpdateLogFilePath(_logFilePath);
        }
    }

    private void UpdateStatusStrip(string message)
    {
        if (statusStrip.InvokeRequired)
        {
            statusStrip.Invoke(new Action(() => toolStripStatusLabel.Text = message));
        }
        else
        {
            toolStripStatusLabel.Text = message;
        }

        // Clear the message after a few seconds (e.g., 3 seconds)
        Task.Delay(3000).ContinueWith(_ => UpdateStatusStrip("Ready"));
    }
}

public class AppData
{
    public Settings Settings { get; set; }
    public Dictionary<string, List<SaveState>> SaveStates { get; set; }
}

public class Settings
{
    public string LogFilePath { get; set; }
}

public class SaveState
{
    public long Created { get; set; }
    public long Timestamp { get; set; }
    public string Base64 { get; set; }
}