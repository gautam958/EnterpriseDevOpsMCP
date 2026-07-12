using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol;
using System;
using System.ComponentModel;
using System.IO;
using System.Diagnostics;
using ModelContextProtocol.Server;

var builder = Host.CreateDefaultBuilder(args);

// CRITICAL: Stdio servers must log to standard error, not standard out.
// Standard out is strictly reserved for the MCP JSON-RPC protocol communication.
builder.ConfigureLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole(consoleLogOptions => 
    {
        consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
    });
});

// Register the MCP Server using Stdio transport and auto-discover tools
builder.ConfigureServices((context, services) =>
{
    services.AddMcpServer()
        .WithStdioServerTransport()
        .WithToolsFromAssembly();
});
 

var host = builder.Build();
await host.RunAsync();

// ====================================================================
// LOCAL CUSTOM TOOLS (The core of your portfolio project)
// ====================================================================

[McpServerToolType]
public static class DevOpsTools
{
    [McpServerTool, Description("Retrieves the current memory usage and active process count of the local development machine.")]
    public static string GetSystemDiagnostics()
    {
        var process = Process.GetCurrentProcess();
        long memoryUsedMB = process.PrivateMemorySize64 / (1024 * 1024);
        int threadCount = process.Threads.Count;
        
        return $"System Diagnostics - Memory Used by MCP Host: {memoryUsedMB} MB. Active Threads: {threadCount}. Host OS: {Environment.OSVersion}";
    }

    [McpServerTool, Description("Reads the last N lines of a local application log file to diagnose recent errors.")]
    public static string ReadLocalLogs(int linesToRead = 10)
    {
        // Simulate reading a local log file (e.g., from a local microservice or IIS)
        string logDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MyAppLogs");
        Directory.CreateDirectory(logDirectory);
        string mockLogFile = Path.Combine(logDirectory, "error.log");

        if (!File.Exists(mockLogFile))
        {
            File.WriteAllText(mockLogFile, "[2026-07-12 10:00:00] INFO: Service started.\n[2026-07-12 10:05:00] ERROR: SQL Server timeout connection.\n");
        }

        var lines = File.ReadAllLines(mockLogFile);
        int skip = Math.Max(0, lines.Length - linesToRead);
        
        return string.Join(Environment.NewLine, lines[skip..]);
    }

    [McpServerTool, Description("Simulates an Azure Cosmos DB or SQL Server query to retrieve tenant configurations.")]
    public static string QueryTenantConfiguration(string tenantId)
    {
        if (tenantId == "T-100")
        {
            return "{ 'TenantId': 'T-100', 'Tier': 'Enterprise', 'Features': ['RAG', 'VectorSearch'], 'Status': 'Active' }";
        }
        return $"{{ 'Error': 'Tenant {tenantId} not found in the local database replica.' }}";
    }
}