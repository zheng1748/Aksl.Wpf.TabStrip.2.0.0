using Aksl.Dialogs.Services;
using Aksl.Toolkit.Controls;
using Microsoft.Extensions.DependencyInjection;
using Prism;
using Prism.Events;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Prism.Unity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Interop;
using Unity;

namespace Aksl.Infrastructure;

public static class JsonSerializerHelper
{
    public static async Task<string> SerializeStringAsync<TValue>(TValue value)
    {
        string json = default;

        await using MemoryStream stream = new();
        await JsonSerializer.SerializeAsync(stream, value);
        stream.Position = 0;
        using StreamReader reader = new(stream);
        json = await reader.ReadToEndAsync();

        return json;
    }
}

