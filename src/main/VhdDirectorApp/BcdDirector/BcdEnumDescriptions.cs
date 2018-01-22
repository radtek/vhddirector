﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VhdDirectorApp.BcdDirector
{
    public class BcdEnumDescriptions
    {
        static public Dictionary<string, string> Descriptions = new Dictionary<string, string>() 
        {
            {"BcdBootMgrString_BcdFilePath", "The boot application."},
            {"BcdBootMgrDevice_BcdDevice", "The device on which the boot application resides."},
            {"BcdBootMgrObjectList_ToolsDisplayOrder", "The boot manager tools display order list."},
            {"BcdBootMgrObject_ResumeObject", "The resume application object."},
            {"BcdBootMgrBoolean_AttemptResume", "Indicates that a resume operation should be attempted during a system restart."},
            {"BcdBootMgrInteger_Timeout", "The maximum number of seconds a boot selection menu is to be displayed to the user. The menu is displayed until the user selects an option or the time-out expires.|If this value is not specified, the boot manager waits for the user to make a selection."},
            {"BcdBootMgrObject_DefaultObject", "The default boot environment application to load if the user does not select one."},
            {"BcdBootMgrObjectList_BootSequence", "List of boot environment applications the boot manager should execute. The applications are executed in the order they appear in this list.|If the firmware boot manager does not support loading multiple applications, this list cannot contain more than one entry."},
            {"BcdBootMgrObjectList_DisplayOrder", "The order in which BCD objects should be displayed. Objects are displayed using the string specified by the BcdLibraryString_Description element."},
            {"BcdDeviceInteger_RamdiskImageLength", "The length of the image for the RAM disk."},
            {"BcdDeviceInteger_SdiPath", "The path from the root of the SDI device to the RAM disk file."},
            {"BcdDeviceInteger_SdiDevice", "The device that contains the SDI object."},
            {"BcdDeviceInteger_TftpClientPort", "The IP port number to be used for TFTP reads.|If this value is not specified, the default TFTP protocol port is used."},
            {"BcdDeviceInteger_RamdiskImageOffset", "The RAM disk image offset."},
            {"BcdLibraryBoolean_AllowPrereleaseSignatures", "Indicates whether the test code signing certificate is supported."},
            {"BcdLibraryInteger_ConfigAccessPolicy", "Indicates the access policy for PCI configuration space.|The following are the possible values. TermDescription ConfigAccessPolicyDefault (0) Access to PCI configuration space through the memory-mapped region is allowed. ConfigAccessPolicyDisallowMmConfig (1) Access to PCI configuration space through the memory-mapped region is not allowed. This setting is used for platforms that implement memory-mapped configuration space incorrectly. The CFC/CF8 access mechanism can be used to access configuration space on these platforms. Â "},
            {"BcdLibraryBoolean_GraphicsModeDisabled", "Indicates whether graphics mode is disabled and boot applications must use text mode display."},
            {"BcdLibraryBoolean_DisplayOptionsEdit", "Indicates whether the boot options editor is enabled."},
            {"BcdLibraryBoolean_DisplayAdvancedOptions", "Indicates whether the advanced options boot menu (F8) is displayed."},
            {"BcdLibraryString_LoadOptionsString", "String that is appended to the load options string passed to the kernel to be consumed by kernel-mode components. This is useful for communicating with kernel-mode components that are not BCD-aware."},
            {"BcdLibraryInteger_EmsBaudRate", "Baud rate for EMS redirection."},
            {"BcdLibraryInteger_EmsPort", "COM port number for EMS redirection.|If this value is not specified, the default is specified by the SPCR ACPI table settings."},
            {"BcdLibraryBoolean_EmsEnabled", "Indicates whether EMS redirection should be enabled."},
            {"BcdLibraryInteger_DebuggerStartPolicy", "Indicates the debugger start policy.|The following are the possible values. TermDescription DebuggerStartActive (0) The debugger will start active. DebuggerStartAutoEnable (1) The debugger will start in the auto-enabled state. If a debugger is attached it will be used; otherwise the debugger port will be available for other applications. DebuggerStartDisable (2) The debugger will not start. Â "},
            {"BcdLibraryBoolean_DebuggerIgnoreUsermodeExceptions", "If TRUE, the debugger will ignore user mode exceptions and only stop for kernel mode exceptions."},
            {"BcdLibraryString_UsbDebuggerTargetName", "The target name for the USB debugger. The target name is arbitrary but must match between the debugger and the debug target."},
            {"BcdLibraryInteger_1394DebuggerChannel", "Channel number for 1394 debugging."},
            {"BcdLibraryInteger_SerialDebuggerBaudRate", "Baud rate for serial debugging.|If this value is not specified, the default is specified by the DBGP ACPI table settings."},
            {"BcdLibraryInteger_SerialDebuggerPort", "Serial port number for serial debugging.|If this value is not specified, the default is specified by the DBGP ACPI table settings."},
            {"BcdLibraryInteger_SerialDebuggerPortAddress", "I/O port address for the serial debugger."},
            {"BcdLibraryInteger_DebuggerType", "Debugger type. The element data format is BcdIntegerElement and the Integer property is one of the values from the BcdLibrary_DebuggerType enumeration."},
            {"BcdLibraryBoolean_DebuggerEnabled", "Indicates whether the boot debugger should be enabled."},
            {"BcdLibraryInteger_FirstMegabytePolicy", "Indicates how the first megabyte of memory is to be used.|The following are the possible values. TermDescription FirstMegabytePolicyUseNone (0) Use none of the first megabyte of memory. FirstMegabytePolicyUseAll (1) Use all of the first megabyte of memory. FirstMegabytePolicyUsePrivate (2) Reserved for future use. Â "},
            {"BcdLibraryBoolean_AllowBadMemoryAccess", "If TRUE, indicates that a boot application can use memory listed in the BcdLibraryIntegerList_BadMemoryList."},
            {"BcdLibraryIntegerList_BadMemoryList", "List of page frame numbers describing faulty memory in the system."},
            {"BcdLibraryBoolean_AutoRecoveryEnabled", "Indicates whether the recovery sequence executes automatically if the boot application fails. Otherwise, the recovery sequence only runs on demand."},
            {"BcdLibraryObjectList_RecoverySequence", "List of boot environment applications to be executed if the associated application fails. The applications are executed in the order they appear in this list."},
            {"BcdLibraryInteger_TruncatePhysicalMemory", "Maximum physical address a boot environment application should recognize. All memory above this address is ignored."},
            {"BcdLibraryObjectList_InheritedObjects", "List of BCD objects from which the current object should inherit elements."},
            {"BcdLibraryString_PreferredLocale", "Preferred locale, in RFC 3066 format."},
            {"BcdLibraryString_Description", "Display name of the boot environment application."},
            {"BcdLibraryString_ApplicationPath", "Path to a boot environment application."},
            {"BcdLibraryDevice_ApplicationDevice", "Device on which a boot environment application resides."},
            {"DebuggerUsb", "USB debugger."},
            {"Debugger1394", "1394 debugger."},
            {"DebuggerSerial", "Serial debugger."},
            {"SafemodeDsRepair", "Boot the system into a repair mode that restores the Active Directory service from backup medium."},
            {"SafemodeNetwork", "Load the drivers and services specified by name or group under the following registry key: HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Control\\SafeBoot\\Network"},
            {"SafemodeMinimal", "Load the drivers and services specified by name or group under the following registry key: HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Control\\SafeBoot\\Minimal."},
            {"BcdMemDiagInteger_FailureCount", "The number of pages that contain errors. This is useful for simulating error flows in the absence of bad physical memory."},
            {"BcdMemDiagInteger_PassCount", "The number of passes for the current test mix.|If this value is not specified, the default is to run memory diagnostic tests until the computer is powered off or the user logs off."},
            {"BcdOSLoaderInteger_BootStatusPolicy", "The boot status policy.|TermDescription BootStatusPolicyDisplayAllFailures (0) Display all boot failures. BootStatusPolicyIgnoreAllFailures (1) Ignore all boot failures. BootStatusPolicyIgnoreShutdownFailures (2) Ignore all shutdown failures. BootStatusPolicyIgnoreBootFailures (3) Ignore all boot failures. Â "},
            {"BcdOSLoaderInteger_DriverLoadFailurePolicy", "Indicates the driver load failure policy. Zero (0) indicates that a failed driver load is fatal and the boot will not continue, one (1) indicates that the standard error control is used."},
            {"BcdOSLoaderBoolean_EmsEnabled", "Indicates whether EMS should be enabled in the kernel."},
            {"BcdOSLoaderBoolean_DebuggerHalBreakpoint", "Indicates whether the HAL should call DbgBreakPoint at the start of HalInitSystem for phase 0 initialization of the kernel."},
            {"BcdOSLoaderBoolean_KernelDebuggerEnabled", "Indicates whether the kernel debugger should be enabled using the settings in the inherited debugger object."},
            {"BcdOSLoaderBoolean_VerboseObjectLoadMode", "Indicates whether the system should display verbose information."},
            {"BcdOSLoaderBoolean_BootLogInitialization", "Indicates whether the system should write logging information to %SystemRoot%\\Ntbtlog.txt during initialization."},
            {"BcdOSLoaderBoolean_SafeBootAlternateShell", "Indicates whether the system should use the shell specified under the following registry key instead of the default shell: HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Control\\SafeBoot\\AlternateShell."},
            {"BcdOSLoaderInteger_SafeBoot", "The element data format is BcdIntegerElement and the Integer property is one of the values from the BcdLibrary_SafeBoot enumeration."},
            {"BcdOSLoaderInteger_MsiPolicy", "The PCI Message Signaled Interrupt (MSI) policy. Zero (0) indicates default, and one (1) indicates that MSI interrupts are disabled."},
            {"BcdOSLoaderInteger_UseFirmwarePciSettings", "Indicates whether the system should use I/O and IRQ resources created by the system firmware instead of using dynamically configured resources."},
            {"BcdOSLoaderBoolean_ProcessorConfigurationFlags", "Indicates whether processor specific configuration flags are to be used."},
            {"BcdOSLoaderBoolean_ForceMaximumProcessors", "Indicates whether the system should use the maximum number of processors."},
            {"BcdOSLoaderInteger_NumberOfProcessors", "The maximum number of processors that can be utilized by the system; all other processors are ignored."},
            {"BcdOSLoaderBoolean_UseBootProcessorOnly", "Indicates whether the operating system should initialize or start non-boot processors."},
            {"BcdOSLoaderInteger_RestrictApicCluster", "The maximum number of APIC clusters that should be used by cluster-mode addressing."},
            {"BcdOSLoaderBoolean_UsePhysicalDestination", "Indicates whether to enable physical-destination mode for all APIC messages."},
            {"BcdOSLoaderInteger_ClusterModeAddressing", "Indicates that cluster-mode APIC addressing should be utilized, and the value is the maximum number of processors per cluster."},
            {"BcdOSLoaderBoolean_DisableVesaBios", "Indicates whether the VGA driver should avoid VESA BIOS calls."},
            {"BcdOSLoaderBoolean_DisableBootDisplay", "Indicates whether the system should initialize the VGA driver responsible for displaying simple graphics during the boot process. If not, there is no display is presented during the boot process."},
            {"BcdOSLoaderBoolean_UseVgaDriver", "Indicates whether the system should use the standard VGA display driver instead of a high-performance display driver."},
            {"BcdOSLoaderInteger_IncreaseUserVa", "The amount of memory that should be utilized by the process address space, in bytes. This value should be between 2GB and 3GB.|Increasing this value from the default 2GB decreases the amount of virtual address space available to the system and device drivers."},
            {"BcdOSLoaderInteger_RemoveMemory", "The amount of memory the system should ignore."},
            {"BcdOSLoaderBoolean_NoLowMemory", "Indicates whether the system should utilize the first 4GB of physical memory. This option requires 5GB of physical memory, and on x86 systems it requires PAE to be enabled."},
            {"BcdOSLoaderBoolean_AllowPrereleaseSignatures", "Indicates whether the test code signing certificate is supported."},
            {"BcdOSLoaderBoolean_UseLastGoodSettings", "Indicates that the system should use the last-known good settings."},
            {"BcdOSLoaderBoolean_DisableCrashAutoReboot", "Indicates that the system should not automatically reboot when it crashes."},
            {"BcdOSLoaderBoolean_WinPEMode", "Indicates that the system should be started in Windows Preinstallation Environment (Windows PE) mode."},
            {"BcdOSLoaderInteger_PAEPolicy", "The Physical Address Extension (PAE) policy. The element data format is BcdIntegerElement and the Integer property is one of the values from the BcdOSLoader_PAEPolicy enumeration. If this value is not specified, the default is PaePolicyDefault."},
            {"BcdOSLoaderInteger_NxPolicy", "The no-execute page protection policy. The element data format is BcdIntegerElement and the Integer property is one of the values from the BcdOSLoader_NxPolicy enumeration. If this value is not specified, the default is NxPolicyAlwaysOff."},
            {"BcdOSLoaderString_DbgTransportPath", "The transport DLL to be loaded by the operating system loader. This value overrides the default Kdcom.dll."},
            {"BcdOSLoaderString_HalPath", "The HAL to be loaded by the operating system loader. This value overrides the default HAL."},
            {"BcdOSLoaderString_KernelPath", "The kernel to be loaded by the operating system loader. This value overrides the default kernel."},
            {"BcdOSLoaderBoolean_DetectKernelAndHal", "Indicates whether the operating system loader should determine the kernel and HAL to load based on the platform features."},
            {"BcdOSLoaderObject_AssociatedResumeObject", "The resume application associated with the operating system."},
            {"BcdOSLoaderString_SystemRoot", "The file path to the operating system (%SystemRoot% minus the volume.)"},
            {"BcdOSLoaderDevice_OSDevice", "The device on which the operating system resides."},
            {"NxPolicyAlwaysOn", "The no-execute page protection is always on."},
            {"NxPolicyAlwaysOff", "The no-execute page protection is always off."},
            {"NxPolicyOptOut", "The no-execute page protection is on by default."},
            {"NxPolicyOptIn", "The no-execute page protection is off by default."},
            {"PaePolicyForceDisable", "PAE is disabled."},
            {"PaePolicyForceEnable", "PAE is enabled."},
            {"PaePolicyDefault", "Enable PAE if hot-pluggable memory is defined above 4GB."}
        };
    }
}