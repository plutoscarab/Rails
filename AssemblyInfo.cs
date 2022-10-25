
// AssemblyInfo.cs

using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("Rails")]
[assembly: AssemblyDescription("Simulation of Mayfair Games' Eurorails™ board game")]
[assembly: AssemblyCompany("Pluto Scarab Software")]
[assembly: AssemblyProduct("Rails")]

[assembly: AssemblyVersion("1.1.*")]

[assembly: AssemblyDelaySign(false)]
[assembly: AssemblyKeyFile("..\\..\\Rails.snk")]
[assembly: AssemblyKeyName("")]

[assembly:FileIOPermission(SecurityAction.RequestMinimum, Unrestricted=true)]
[assembly:ComVisible(false)]
[assembly:CLSCompliant(true)]