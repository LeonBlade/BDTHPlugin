using System;

namespace BDTHPlugin
{
  public class PluginException : Exception
  {
    public PluginException() { }
    public PluginException(string message) : base($"BDTH Exception: {message}") { }
  }
}
