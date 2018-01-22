﻿//Josip Medved <jmedved@jmedved.com>  http://www.jmedved.com  http://medo64.blogspot.com

//2009-04-07: First version.
//2009-04-30: Updated for Windows 7 release candidate.
//2009-05-03: Fixed bug with wrong RWDepth.
//2009-06-12: Fixed ATTACH_VIRTUAL_DISK_FLAG enumeration change from Windows 7 beta to RC.


// sfinktah's notes:

// http://msdn.microsoft.com/en-us/library/windows/desktop/dd323699%28v=VS.85%29.aspx
// VHD Functions  (virtdisk.dll)

// AttachVirtualDisk
// BreakMirrorVirtualDisk
// CompactVirtualDisk
// CreateVirtualDisk
// DetachVirtualDisk
// ExpandVirtualDisk
// GetStorageDependencyInformation
// GetVirtualDiskInformation
// GetVirtualDiskOperationProgress
// GetVirtualDiskPhysicalPath
// MergeVirtualDisk
// MirrorVirtualDisk
// OpenVirtualDisk
// SetVirtualDiskInformation


// end sfinktah's notes
using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Threading;


namespace Medo.IO {

    /// <summary>
    /// Manipulation with Virtual Disk files.
    /// </summary>
    public class VirtualDisk : IDisposable {

        private NativeMethods.VirtualDiskSafeHandle _handle = new NativeMethods.VirtualDiskSafeHandle();
        private NativeOverlapped _createOverlap;
        private ManualResetEvent _createOverlapEvent;
        private bool _attached;

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="fileName">Full path of VHD file.</param>
        public VirtualDisk(string fileName) {
            this.FileName = fileName;
        }

        /// <summary>
        /// Gets file name of VHD.
        /// </summary>
        public string FileName { get; private set; }

        /// <summary>
        /// Gets whether connection to file is currently open.
        /// </summary>
        public bool IsOpen {
            [SecurityPermission(SecurityAction.Demand)]
            get { return !(this._handle.IsClosed || this._handle.IsInvalid); }
        }

        /// <summary>
        /// Opens connection to file.
        /// </summary>
        [SecurityPermission(SecurityAction.Demand)]
        public void Open(NativeMethods.VIRTUAL_DISK_ACCESS_MASK AccessMask = NativeMethods.VIRTUAL_DISK_ACCESS_MASK.VIRTUAL_DISK_ACCESS_ALL)
        {
            if (CSharp.cc.Files.CheckIfFileIsBeingUsed(this.FileName))
            {
                CSharp.cc.DebugConsole.WriteLine("{0} was being used (VirtualDisk.Open)", this.FileName);
            }
            else
            {
                CSharp.cc.DebugConsole.WriteLine("{0} opened (VirtualDisk.Open)", this.FileName);

            }
            var parameters = new NativeMethods.OPEN_VIRTUAL_DISK_PARAMETERS();
            parameters.Version = NativeMethods.OPEN_VIRTUAL_DISK_VERSION.OPEN_VIRTUAL_DISK_VERSION_1;
            parameters.Version1.RWDepth = NativeMethods.OPEN_VIRTUAL_DISK_RW_DEPTH_DEFAULT;

            var storageType = new NativeMethods.VIRTUAL_STORAGE_TYPE();
            storageType.DeviceId = NativeMethods.VIRTUAL_STORAGE_TYPE_DEVICE_VHD;
            storageType.VendorId = NativeMethods.VIRTUAL_STORAGE_TYPE_VENDOR_MICROSOFT;

            int res = NativeMethods.OpenVirtualDisk(ref storageType, this.FileName, 
                // NativeMethods.VIRTUAL_DISK_ACCESS_MASK.VIRTUAL_DISK_ACCESS_ALL,
                AccessMask,
                NativeMethods.OPEN_VIRTUAL_DISK_FLAG.OPEN_VIRTUAL_DISK_FLAG_NONE, ref parameters, ref _handle);
            if (res == NativeMethods.ERROR_SUCCESS) {
            } else {
                _handle.SetHandleAsInvalid();
                if ((res == NativeMethods.ERROR_FILE_NOT_FOUND) || (res == NativeMethods.ERROR_PATH_NOT_FOUND)) {
                    throw new System.IO.FileNotFoundException("File not found.");
                } else if (res == NativeMethods.ERROR_FILE_CORRUPT) {
                    throw new System.IO.InvalidDataException("File format not recognized.");
                } else {
                    throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Native error {0}.", res));
                }
            }
        }

        /// <summary>
        /// Creates new virtual disk
        /// </summary>
        /// <param name="size">Size in bytes.</param>
        [SecurityPermission(SecurityAction.Demand)]
        public void Create(long size) {
            Create(size, VirtualDiskCreateOptions.None, 0, 0, false);
        }

        /// <summary>
        /// Creates new virtual disk
        /// </summary>
        /// <param name="size">Size in bytes.</param>
        /// <param name="blockSize">Block size in bytes. If value is 0, default is used.</param>
        /// <param name="sectorSize">Sector size in bytes. If value is 0, default is used.</param>
        [SecurityPermission(SecurityAction.Demand)]
        public void Create(long size, VirtualDiskCreateOptions options) {
            this.Create(size, options, 0, 0, false);
        }

        /// <summary>
        /// Creates new virtual disk
        /// </summary>
        /// <param name="size">Size in bytes.</param>
        /// <param name="options">Additional options.</param>
        /// <param name="blockSize">Block size in bytes. If value is 0, default is used.</param>
        /// <param name="sectorSize">Sector size in bytes. If value is 0, default is used.</param>
        [SecurityPermission(SecurityAction.Demand)]
        public void Create(long size, VirtualDiskCreateOptions options, int blockSize, int sectorSize) {
            this.Create(size, options, blockSize, sectorSize, false);
        }

        /// <summary>
        /// Creates new virtual disk
        /// </summary>
        /// <param name="size">Size in bytes.</param>
        [SecurityPermission(SecurityAction.Demand)]
        public void CreateAsync(long size) {
            Create(size, VirtualDiskCreateOptions.None, 0, 0, true);
        }

        /// <summary>
        /// Creates new virtual disk
        /// </summary>
        /// <param name="size">Size in bytes.</param>
        /// <param name="options">Additional options.</param>
        [SecurityPermission(SecurityAction.Demand)]
        public void CreateAsync(long size, VirtualDiskCreateOptions options) {
            this.Create(size, options, 0, 0, true);
        }

        /// <summary>
        /// Creates new virtual disk
        /// </summary>
        /// <param name="size">Size in bytes.</param>
        /// <param name="options">Additional options.</param>
        /// <param name="blockSize">Block size in bytes. If value is 0, default is used.</param>
        /// <param name="sectorSize">Sector size in bytes. If value is 0, default is used.</param>
        [SecurityPermission(SecurityAction.Demand)]
        public void CreateAsync(long size, VirtualDiskCreateOptions options, int blockSize, int sectorSize) {
            this.Create(size, options, blockSize, sectorSize, true);
        }

        /// <summary>
        /// Creates new virtual disk
        /// </summary>
        /// <param name="size">Size in bytes.</param>
        /// <param name="options">Additional options.</param>
        /// <param name="blockSize">Block size in bytes. If value is 0, default is used.</param>
        /// <param name="sectorSize">Sector size in bytes. If value is 0, default is used.</param>
        /// <param name="createAsync">True if creation process is to be performed asynchronous.</param>
        [SecurityPermission(SecurityAction.Demand)]
        private void Create(long size, VirtualDiskCreateOptions options, int blockSize, int sectorSize, bool createAsync) {
            var parameters = new NativeMethods.CREATE_VIRTUAL_DISK_PARAMETERS();
            parameters.Version = NativeMethods.CREATE_VIRTUAL_DISK_VERSION.CREATE_VIRTUAL_DISK_VERSION_1;
            if (blockSize == 0) {
                //parameters.Version1.BlockSizeInBytes = NativeMethods.CREATE_VIRTUAL_DISK_PARAMETERS_DEFAULT_BLOCK_SIZE; //blocks if you set it
                parameters.Version1.BlockSizeInBytes = 0;
            } else {
                parameters.Version1.BlockSizeInBytes = blockSize;
            }
            parameters.Version1.MaximumSize = size;
            parameters.Version1.ParentPath = IntPtr.Zero;
            if (sectorSize == 0) {
                parameters.Version1.SectorSizeInBytes = NativeMethods.CREATE_VIRTUAL_DISK_PARAMETERS_DEFAULT_SECTOR_SIZE;
            } else {
                parameters.Version1.SectorSizeInBytes = sectorSize;
            }
            parameters.Version1.SourcePath = IntPtr.Zero;
            parameters.Version1.UniqueId = Guid.Empty;

            var storageType = new NativeMethods.VIRTUAL_STORAGE_TYPE();
            storageType.DeviceId = NativeMethods.VIRTUAL_STORAGE_TYPE_DEVICE_VHD;
            storageType.VendorId = NativeMethods.VIRTUAL_STORAGE_TYPE_VENDOR_MICROSOFT;

            int res;
            if (createAsync) {
                this._createOverlapEvent = new ManualResetEvent(false);
                this._createOverlap = new NativeOverlapped();
                this._createOverlap.OffsetLow = 0;
                this._createOverlap.OffsetHigh = 0;
                this._createOverlap.EventHandle = this._createOverlapEvent.Handle;
                res = NativeMethods.CreateVirtualDisk(ref storageType, this.FileName, NativeMethods.VIRTUAL_DISK_ACCESS_MASK.VIRTUAL_DISK_ACCESS_ALL, IntPtr.Zero, (NativeMethods.CREATE_VIRTUAL_DISK_FLAG)options, 0, ref parameters, ref _createOverlap, ref _handle);
            } else {
                res = NativeMethods.CreateVirtualDisk(ref storageType, this.FileName, NativeMethods.VIRTUAL_DISK_ACCESS_MASK.VIRTUAL_DISK_ACCESS_ALL, IntPtr.Zero, (NativeMethods.CREATE_VIRTUAL_DISK_FLAG)options, 0, ref parameters, IntPtr.Zero, ref _handle);
            }
            if (res == NativeMethods.ERROR_SUCCESS) {
            } else if (res == NativeMethods.ERROR_IO_PENDING) {
            } else {
                _handle.SetHandleAsInvalid();
                if ((res == NativeMethods.ERROR_FILE_NOT_FOUND) || (res == NativeMethods.ERROR_PATH_NOT_FOUND)) {
                    throw new System.IO.FileNotFoundException("File not found.");
                } else if (res == NativeMethods.ERROR_FILE_CORRUPT) {
                    throw new System.IO.InvalidDataException("File format not recognized.");
                } else if (res == NativeMethods.ERROR_FILE_EXISTS) {
                    throw new System.IO.IOException("File already exists.");
                } else if (res == NativeMethods.ERROR_INVALID_PARAMETER) {
                    throw new System.ArgumentException("Invalid parameter.", "size");
                } else {
                    throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Native error {0}.", res));
                }
            }
        }

        /// <summary>
        /// Returns progress for create operation.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Method may throw exceptions.")]
        public VirtualDiskOperationProgress GetCreateProgress() {
            var progress = new NativeMethods.VIRTUAL_DISK_PROGRESS();
            int res = NativeMethods.GetVirtualDiskOperationProgress(this._handle, ref this._createOverlap, ref progress);
            if (res == NativeMethods.ERROR_SUCCESS) {
                res = progress.OperationStatus; //overwrites original res in order for copy/paste error handling to work (copy/paste is from Create method and may be used in future also)
                if (res == NativeMethods.ERROR_SUCCESS) {
                    return new VirtualDiskOperationProgress(100, true);
                } else if (res == NativeMethods.ERROR_IO_PENDING) {
                    if (this._createOverlapEvent.WaitOne(0)) {
                        return new VirtualDiskOperationProgress(100, true);
                    } else {
                        return new VirtualDiskOperationProgress((int)((progress.CurrentValue * 100) / progress.CompletionValue), false);
                    }
                } else {
                    if ((res == NativeMethods.ERROR_FILE_NOT_FOUND) || (res == NativeMethods.ERROR_PATH_NOT_FOUND)) {
                        throw new System.IO.FileNotFoundException("File not found.");
                    } else if (res == NativeMethods.ERROR_FILE_CORRUPT) {
                        throw new System.IO.InvalidDataException("File format not recognized.");
                    } else if (res == NativeMethods.ERROR_FILE_EXISTS) {
                        throw new System.IO.IOException("File already exists.");
                    } else {
                        throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Native error {0}.", res));
                    }
                }
            } else {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Native error {0}.", res));
            }
        }

        /// <summary>
        /// Attaches opened file.
        /// </summary>
        /// <param name="options">Options to use when attaching.</param>
        public void Attach(VirtualDiskAttachOptions options) {
            if (_attached)
            {
                throw new InvalidOperationException("Already attached.");
            }
            var parameters = new NativeMethods.ATTACH_VIRTUAL_DISK_PARAMETERS();
            parameters.Version = NativeMethods.ATTACH_VIRTUAL_DISK_VERSION.ATTACH_VIRTUAL_DISK_VERSION_1;

            int res = NativeMethods.AttachVirtualDisk(_handle, IntPtr.Zero, (NativeMethods.ATTACH_VIRTUAL_DISK_FLAG)options, 0, ref parameters, IntPtr.Zero);
            if (res == NativeMethods.ERROR_SUCCESS) {
                _attached = true;
            } else if (res == NativeMethods.ERROR_ACCESS_DENIED) {
                throw new System.IO.IOException("Access is denied.");
            } else if (res == NativeMethods.ERROR_INVALID_HANDLE) {
                throw new InvalidOperationException("The handle is invalid.");
            } else if (res == NativeMethods.ERROR_PRIVILEGE_NOT_HELD) {
                throw new System.UnauthorizedAccessException("A required privilege is not held by the client.");
            } else {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Native error {0}.", res));
            }
        }

        /// <summary>
        /// Returns path of attached disk.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "This method may not return immediately when called.")]
        public string GetAttachedPath() {
            int res = 0;
            int pathSize = 200; //size in bytes (200) - not in chars (100)
            var path = new StringBuilder(pathSize / 2); //unicode

            res = NativeMethods.GetVirtualDiskPhysicalPath(_handle, ref pathSize, path);
            if (res == NativeMethods.ERROR_INSUFFICIENT_BUFFER) {
                path.Capacity = pathSize / 2;
                res = NativeMethods.GetVirtualDiskPhysicalPath(_handle, ref pathSize, path);
            }

            if (res == NativeMethods.ERROR_SUCCESS) {
                return path.ToString(0, pathSize / 2 - 1); //unicode
            } else if (res == NativeMethods.ERROR_DEV_NOT_EXIST) {
                throw new IOException("Device could not be accessed.");
            } else {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Native error {0}.", res));
            }
        }

        /// <summary>
        /// Detaches disk from file system.
        /// </summary>
        public void Detach() {
            if (!_attached)
            {
                return;
                throw new InvalidOperationException("Attempted to Detach() when not attached.");
            }

            // If the DetachVirtualDisk function fails with an error code value of ERROR_INVALID_PARAMETER, the cause may be due to any of the following conditions:

            //The VirtualDiskHandle parameter is not a valid handle created by the OpenVirtualDisk function.
            //The Flags parameter is set to a value other than DETACH_VIRTUAL_DISK_FLAG_NONE (0).

            //The host volume that contains the virtual disk image file cannot be compressed or EFS encrypted.

            //All other open handles to the virtual disk must be closed before the DetachVirtualDisk function can succeed.

            //If the virtual disk is attached and another handle that was used to attach it has been closed, this is because the ATTACH_VIRTUAL_DISK_FLAG_PERMANENT_LIFETIME flag was specified. In this case, the DetachVirtualDisk function can succeed but the VHD will remain attached. If the ATTACH_VIRTUAL_DISK_FLAG_PERMANENT_LIFETIME was not specified, the virtual disk will be automatically detached when the last open handle is closed.

            //This function will fail if a provider cannot be found, if the image file is not valid, if the image is not attached, or if the caller does not have SE_MANAGE_VOLUME_PRIVILEGE access rights on a Windows Server operating system. For more information about file security, see File Security and Access Rights.

            int res = NativeMethods.DetachVirtualDisk(_handle, NativeMethods.DETACH_VIRTUAL_DISK_FLAG.DETACH_VIRTUAL_DISK_FLAG_NONE, 0);
            if (res == NativeMethods.ERROR_SUCCESS) {
                _attached = false;
                // If there is an error detaching, then our status is dubious, but should probably stay "_attached" - sfinktah
            } else if (res == NativeMethods.ERROR_INVALID_HANDLE) {
                throw new InvalidOperationException("The handle is invalid.");
            } else if (res == NativeMethods.ERROR_PRIVILEGE_NOT_HELD) {
                throw new System.UnauthorizedAccessException("A required privilege is not held by the client.");
            } else if (res == NativeMethods.ERROR_NOT_FOUND) {
                throw new System.NotSupportedException("Element not found.");
            } else {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Native error {0}.", res));
            }
        }


        /// <summary>
        /// Closes connection to file.
        /// </summary>
        [SecurityPermission(SecurityAction.Demand)]
        public void Close() {
            //if (this.IsAttached)
            //{
            //    this.Detach();
            //}
            if (this.IsOpen) {
                this._attached = false;
                this._handle.Close();
            }
        }


        // sfinktah's additions start here

        /// <summary>
        /// Gets whether connection to file is currently open.
        /// </summary>
        public bool IsAttached
        {
            [SecurityPermission(SecurityAction.Demand)]
            get { return !(this._handle.IsClosed || this._handle.IsInvalid) && this._attached; }
        }

        [SecurityPermission(SecurityAction.Demand)]
        public int Expand(long size)
        {  // public static extern Int32 ExpandVirtualDisk(VirtualDiskSafeHandle VirtualDiskHandle, EXPAND_VIRTUAL_DISK_FLAG Flags, ref EXPAND_VIRTUAL_DISK_PARAMETERS Parameters, IntPtr Overlapped);
        
            //        VirtualDiskHandle [in]

            //    A handle to the open virtual disk, which must have been opened using the VIRTUAL_DISK_ACCESS_METAOPS flag. For information on how to open a virtual disk, see the OpenVirtualDisk function.
            //Flags [in]

            //    Must be the EXPAND_VIRTUAL_DISK_FLAG_NONE value of the EXPAND_VIRTUAL_DISK_FLAG enumeration.
            //Parameters [in]

            //    A pointer to a valid EXPAND_VIRTUAL_DISK_PARAMETERS structure that contains expansion parameter data.
            //Overlapped [in]

            //    An optional pointer to a valid OVERLAPPED structure if asynchronous operation is desired.


            if (this.IsAttached)
            {
                this.Detach();
            }
            if (this.IsOpen)
            {
                // this.Close();
            }

            this.Open(NativeMethods.VIRTUAL_DISK_ACCESS_MASK.VIRTUAL_DISK_ACCESS_METAOPS);
            // this.Attach(VirtualDiskAttachOptions.ReadOnly);                // NativeMethods.ATTACH_VIRTUAL_DISK_FLAG.ATTACH_VIRTUAL_DISK_FLAG_READ_ONLY);


            NativeMethods.EXPAND_VIRTUAL_DISK_PARAMETERS expandParams = new NativeMethods.EXPAND_VIRTUAL_DISK_PARAMETERS();
            expandParams.Version1.NewSize = size;

            int rv = NativeMethods.ExpandVirtualDisk(this._handle, NativeMethods.EXPAND_VIRTUAL_DISK_FLAG.EXPAND_VIRTUAL_DISK_FLAG_NONE,
                    ref expandParams, (IntPtr)null);
            return rv;
        }

        public enum CompactionTypes
        {
            NONE,
            FILE_SYSTEM_AWARE = 0x001,
            FILE_SYSTEM_AGNOSTIC = 0x002,
            ALL = 0x003
        }
        [SecurityPermission(SecurityAction.Demand)]
        public int Compact(CompactionTypes compactionType)
        {
            // VirtualDiskHandle [in]
				//      A handle to the open virtual disk, which must have been
				//      opened using the VIRTUAL_DISK_ACCESS_METAOPS flag in the
				//      VirtualDiskAccessMask parameter passed to OpenVirtualDisk.

            // Flags [in]
				//      Must be the COMPACT_VIRTUAL_DISK_FLAG_NONE value (0) of the
				//      COMPACT_VIRTUAL_DISK_FLAG enumeration.

            // Parameters [in]
				//      A pointer to a valid COMPACT_VIRTUAL_DISK_PARAMETERS
				//      structure that contains compaction parameter data.

            // Overlapped [in]
				//      An optional pointer to a valid OVERLAPPED structure if
				//      asynchronous operation is desired.

            // Returns
            //          Status of the request.

            // If the function succeeds, the return value is ERROR_SUCCESS.
            // If the function fails, the return value is an error code. For more information, see System Error Codes.


            //***
				// Compaction can be run only on a virtual disk that is dynamically
				// expandable or differencing.

            // There are two different types of compaction.

				//     The first type, file-system-aware compaction, uses the NTFS
				//     file system to determine free space. This is done by
				//     attaching the VHD as a read-only device by including the
				//     VIRTUAL_DISK_ACCESS_METAOPS and
				//     VIRTUAL_DISK_ACCESS_ATTACH_RO flags in the
				//     VirtualDiskAccessMask parameter passed to OpenVirtualDisk,
				//     attaching the VHD by calling AttachVirtualDisk, and while
				//     the VHD is attached calling CompactVirtualDisk. Detaching
				//     the VHD before compaction is done can cause compaction to
				//     return failure before it is done (similar to cancellation of
				//     compaction).
				//
				//     The second type, file-system-agnostic compaction, does not
				//     involve the file system but only looks for VHD blocks filled
				//     entirely with zeros (0). This is done by including the
				//     VIRTUAL_DISK_ACCESS_METAOPS flag in the
				//     VirtualDiskAccessMask parameter passed to OpenVirtualDisk,
				//     and calling CompactVirtualDisk.

				// File-system-aware compaction is the most efficient compaction
				// type but using first the file-system-aware compaction followed
				// by the file-system-agnostic compaction will produce the smallest
				// VHD.

				// A compaction operation on a virtual disk can be safely
				// interrupted and re-run later. Re-opening a virtual disk file
				// that has been interrupted may result in the reduction of a
				// virtual disk file's size at the time of opening.

				// Compaction can be CPU-intensive and/or I/O-intensive, depending
				// on how large the virtual disk is and how many blocks require
				// movement.

				// The CompactVirtualDisk function runs on the virtual disk in the
				// same security context as the caller.
            //***


            int rv = 0;
            int iCompactionType = (int)compactionType;

            if ((((int)(CompactionTypes.FILE_SYSTEM_AWARE)) & iCompactionType) > 0)
            {

                if (this.IsAttached)
                {
                    // This generally fails, you should probably always call Compact() with a fresh handle.
                    this.Detach();
                }
                if (this.IsOpen)
                {
                    // We can't close ourselves.
                    // this.Close();
                }

                this.Open(NativeMethods.VIRTUAL_DISK_ACCESS_MASK.VIRTUAL_DISK_ACCESS_ATTACH_RO | NativeMethods.VIRTUAL_DISK_ACCESS_MASK.VIRTUAL_DISK_ACCESS_METAOPS);
                this.Attach(VirtualDiskAttachOptions.ReadOnly);                // NativeMethods.ATTACH_VIRTUAL_DISK_FLAG.ATTACH_VIRTUAL_DISK_FLAG_READ_ONLY);

                NativeMethods.COMPACT_VIRTUAL_DISK_PARAMETERS compactParams = new NativeMethods.COMPACT_VIRTUAL_DISK_PARAMETERS();
                compactParams.Version = NativeMethods.COMPACT_VIRTUAL_DISK_VERSION.COMPACT_VIRTUAL_DISK_VERSION_1;

                rv = NativeMethods.CompactVirtualDisk(this._handle, NativeMethods.COMPACT_VIRTUAL_DISK_FLAG.COMPACT_VIRTUAL_DISK_FLAG_NONE,
                        ref compactParams, (IntPtr)null);
                return rv;

            }


            if ((((int)(CompactionTypes.FILE_SYSTEM_AGNOSTIC)) & iCompactionType) > 0)
            {

                if (this.IsAttached)
                {
                    // This generally fails, you should probably always call Compact() with a fresh handle.
                    this.Detach();
                }
                if (this.IsOpen)
                {
                   // We can't close ourselves.
                   // this.Close();
                }

                this.Open(NativeMethods.VIRTUAL_DISK_ACCESS_MASK.VIRTUAL_DISK_ACCESS_METAOPS);
                // this.Attach(VirtualDiskAttachOptions.ReadOnly);                // NativeMethods.ATTACH_VIRTUAL_DISK_FLAG.ATTACH_VIRTUAL_DISK_FLAG_READ_ONLY);

                NativeMethods.COMPACT_VIRTUAL_DISK_PARAMETERS compactParams = new NativeMethods.COMPACT_VIRTUAL_DISK_PARAMETERS();
                compactParams.Version = NativeMethods.COMPACT_VIRTUAL_DISK_VERSION.COMPACT_VIRTUAL_DISK_VERSION_1;

                rv = NativeMethods.CompactVirtualDisk(this._handle, NativeMethods.COMPACT_VIRTUAL_DISK_FLAG.COMPACT_VIRTUAL_DISK_FLAG_NONE,
                        ref compactParams, (IntPtr)null);

            }

            return rv;
        }




        [SecurityPermission(SecurityAction.Demand)]
        public string GetPhysicalPath()
        {
            // public static extern Int32 GetVirtualDiskPhysicalPath(VirtualDiskSafeHandle VirtualDiskHandle, ref Int32 DiskPathSizeInBytes, StringBuilder DiskPath);

            int len = 1024;
            StringBuilder sb = new StringBuilder(len);
            int rv = NativeMethods.GetVirtualDiskPhysicalPath(this._handle, ref len, sb);
            if (rv == NativeMethods.ERROR_SUCCESS)
            {
                return sb.ToString();
            }

            return string.Empty;
        }

        #region IDisposable Members

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        public void Dispose() {
            this.Dispose(true);
            System.GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">True if managed resources should be disposed; otherwise, false.</param>
        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                this.Close();
            }
        }

        #endregion


        public static class NativeMethods {

            #region VHD Constants

            /// <summary>
            /// Default block size constant definition.
            /// </summary>
            public const int CREATE_VIRTUAL_DISK_PARAMETERS_DEFAULT_BLOCK_SIZE = 0x80000;

            /// <summary>
            /// The default and only allowable size, 512 bytes.
            /// </summary>
            public const int CREATE_VIRTUAL_DISK_PARAMETERS_DEFAULT_SECTOR_SIZE = 0x200;

            /// <summary>
            /// Default value to use if no other value is desired.
            /// </summary>
            public const int OPEN_VIRTUAL_DISK_RW_DEPTH_DEFAULT = 1;

            /// <summary>
            /// </summary>
            public const int VIRTUAL_STORAGE_TYPE_DEVICE_UNKNOWN = 0;

            /// <summary>
            /// </summary>
            public const int VIRTUAL_STORAGE_TYPE_DEVICE_ISO = 1;

            /// <summary>
            /// </summary>
            public const int VIRTUAL_STORAGE_TYPE_DEVICE_VHD = 2;

            /// <summary>
            /// </summary>
            public static readonly Guid VIRTUAL_STORAGE_TYPE_VENDOR_MICROSOFT = new Guid("EC984AEC-A0F9-47e9-901F-71415A66345B");

            /// <summary>
            /// </summary>
            public static readonly Guid VIRTUAL_STORAGE_TYPE_VENDOR_UNKNOWN = Guid.Empty;

            #endregion


            #region VHD Enumerations

            /// <summary>
            /// Contains virtual disk attach request flags.
            /// </summary>
            public enum ATTACH_VIRTUAL_DISK_FLAG : int {
                /// <summary>
                /// No flags. Use system defaults.
                /// </summary>
                ATTACH_VIRTUAL_DISK_FLAG_NONE = 0x00000000,

                /// <summary>
                /// Attach the virtual disk as read-only.
                /// </summary>
                ATTACH_VIRTUAL_DISK_FLAG_READ_ONLY = 0x00000001,

                /// <summary>
                /// Will cause all volumes on the attached virtual disk to be mounted without assigning drive letters to them.
                /// </summary>
                ATTACH_VIRTUAL_DISK_FLAG_NO_DRIVE_LETTER = 0x00000002,

                /// <summary>
                /// Will decouple the virtual disk lifetime from that of the VirtualDiskHandle. The virtual disk will be attached until the UnsurfaceVirtualDisk function is called, even if all open handles to the virtual disk are closed.
                /// </summary>
                ATTACH_VIRTUAL_DISK_FLAG_PERMANENT_LIFETIME = 0x00000004,

                /// <summary>
                /// Reserved.
                /// </summary>
                ATTACH_VIRTUAL_DISK_FLAG_NO_LOCAL_HOST = 0x00000008
            }

            /// <summary>
            /// Contains the version of the virtual hard disk (VHD) ATTACH_VIRTUAL_DISK_PARAMETERS structure to use in calls to VHD functions.
            /// </summary>
            public enum ATTACH_VIRTUAL_DISK_VERSION : int {
                /// <summary>
                /// </summary>
                ATTACH_VIRTUAL_DISK_VERSION_UNSPECIFIED = 0,

                /// <summary>
                /// </summary>
                ATTACH_VIRTUAL_DISK_VERSION_1 = 1
            }

            /// <summary>
            /// Contains virtual hard disk (VHD) compact request flags.
            /// </summary>
            public enum COMPACT_VIRTUAL_DISK_FLAG : int {
                /// <summary>
                /// </summary>
                COMPACT_VIRTUAL_DISK_FLAG_NONE = 0x00000000
            }

            /// <summary>
            /// Contains the version of the virtual hard disk (VHD) COMPACT_VIRTUAL_DISK_PARAMETERS structure to use in calls to VHD functions.
            /// </summary>
            public enum COMPACT_VIRTUAL_DISK_VERSION : int {
                /// <summary>
                /// </summary>
                COMPACT_VIRTUAL_DISK_VERSION_UNSPECIFIED = 0,

                /// <summary>
                /// </summary>
                COMPACT_VIRTUAL_DISK_VERSION_1 = 1
            }

            /// <summary>
            /// Contains virtual disk creation flags.
            /// </summary>
            public enum CREATE_VIRTUAL_DISK_FLAG : int {
                /// <summary>
                /// No special creation conditions; system defaults are used.
                /// </summary>
                CREATE_VIRTUAL_DISK_FLAG_NONE = 0x00000000,

                /// <summary>
                /// Pre-allocate all physical space necessary for the size of the virtual disk.
                /// </summary>
                CREATE_VIRTUAL_DISK_FLAG_FULL_PHYSICAL_ALLOCATION = 0x00000001
            }

            /// <summary>
            /// Contains the version of the virtual hard disk (VHD) CREATE_VIRTUAL_DISK_PARAMETERS structure to use in calls to VHD functions.
            /// </summary>
            public enum CREATE_VIRTUAL_DISK_VERSION : int {
                /// <summary>
                /// </summary>
                CREATE_VIRTUAL_DISK_VERSION_UNSPECIFIED = 0,

                /// <summary>
                /// </summary>
                CREATE_VIRTUAL_DISK_VERSION_1 = 1
            }

            /// <summary>
            /// Contains virtual disk dependency information flags.
            /// </summary>
            public enum DEPENDENT_DISK_FLAG : int {
                /// <summary>
                /// No flags specified. Use system defaults.
                /// </summary>
                DEPENDENT_DISK_FLAG_NONE = 0x00000000,

                /// <summary>
                /// Multiple files backing the virtual disk.
                /// </summary>
                DEPENDENT_DISK_FLAG_MULT_BACKING_FILES = 0x00000001,

                /// <summary>
                /// Fully allocated virtual disk.
                /// </summary>
                DEPENDENT_DISK_FLAG_FULLY_ALLOCATED = 0x00000002,

                /// <summary>
                /// Read-only virtual disk.
                /// </summary>
                DEPENDENT_DISK_FLAG_READ_ONLY = 0x00000004,

                /// <summary>
                /// The backing file of the virtual disk is not on a local physical disk.
                /// </summary>
                DEPENDENT_DISK_FLAG_REMOTE = 0x00000008,

                /// <summary>
                /// Reserved.
                /// </summary>
                DEPENDENT_DISK_FLAG_SYSTEM_VOLUME = 0x00000010,

                /// <summary>
                /// The backing file of the virtual disk is on the system volume.
                /// </summary>
                DEPENDENT_DISK_FLAG_SYSTEM_VOLUME_PARENT = 0x00000020,

                /// <summary>
                /// The backing file of the virtual disk is on a removable physical disk.
                /// </summary>
                DEPENDENT_DISK_FLAG_REMOVABLE = 0x00000040,

                /// <summary>
                /// Drive letters are not automatically assigned to the volumes on the virtual disk.
                /// </summary>
                DEPENDENT_DISK_FLAG_NO_DRIVE_LETTER = 0x00000080,

                /// <summary>
                /// The virtual disk is a parent of a differencing chain.
                /// </summary>
                DEPENDENT_DISK_FLAG_PARENT = 0x00000100,

                /// <summary>
                /// The virtual disk is not surfaced on (attached to) the local host. For example, it is attached to a guest virtual machine.
                /// </summary>
                DEPENDENT_DISK_FLAG_NO_HOST_DISK = 0x00000200,

                /// <summary>
                /// The lifetime of the virtual disk is not tied to any application or process.
                /// </summary>
                DEPENDENT_DISK_FLAG_PERMANENT_LIFETIME = 0x00000400
            }

            /// <summary>
            /// Contains virtual hard disk (VHD) detach request flags.
            /// </summary>
            public enum DETACH_VIRTUAL_DISK_FLAG : int {
                /// <summary>
                /// No flags. Use system defaults.
                /// </summary>
                DETACH_VIRTUAL_DISK_FLAG_NONE = 0x00000000
            }

            /// <summary>
            /// Contains virtual hard disk (VHD) expand request flags.
            /// </summary>
            public enum EXPAND_VIRTUAL_DISK_FLAG : int {
                /// <summary>
                /// </summary>
                EXPAND_VIRTUAL_DISK_FLAG_NONE = 0x00000000
            }

            /// <summary>
            /// Contains the version of the virtual hard disk (VHD) EXPAND_VIRTUAL_DISK_PARAMETERS structure to use in calls to VHD functions.
            /// </summary>
            public enum EXPAND_VIRTUAL_DISK_VERSION : int {
                /// <summary>
                /// </summary>
                EXPAND_VIRTUAL_DISK_VERSION_UNSPECIFIED = 0,

                /// <summary>
                /// </summary>
                EXPAND_VIRTUAL_DISK_VERSION_1 = 1
            }

            /// <summary>
            /// Contains virtual hard disk (VHD) storage dependency request flags.
            /// </summary>
            public enum GET_STORAGE_DEPENDENCY_FLAG : int {
                /// <summary>
                /// No flags specified.
                /// </summary>
                GET_STORAGE_DEPENDENCY_FLAG_NONE = 0x00000000,

                /// <summary>
                /// Return information for volumes or disks hosting the volume specified. 
                /// </summary>
                GET_STORAGE_DEPENDENCY_FLAG_HOST_VOLUMES = 0x00000001,

                /// <summary>
                /// The handle provided is to a disk, not a volume or file.
                /// </summary>
                GET_STORAGE_DEPENDENCY_FLAG_DISK_HANDLE = 0x00000002
            }

            /// <summary>
            /// Contains virtual hard disk (VHD) information retrieval identifiers.
            /// </summary>
            public enum GET_VIRTUAL_DISK_INFO_VERSION : int {
                /// <summary>
                /// Unspecified.
                /// </summary>
                GET_VIRTUAL_DISK_INFO_UNSPECIFIED = 0,

                /// <summary>
                /// Size.
                /// </summary>
                GET_VIRTUAL_DISK_INFO_SIZE = 1,

                /// <summary>
                /// Unique identifier.
                /// </summary>
                GET_VIRTUAL_DISK_INFO_IDENTIFIER = 2,

                /// <summary>
                /// Location of the parent.
                /// </summary>
                GET_VIRTUAL_DISK_INFO_PARENT_LOCATION = 3,

                /// <summary>
                /// Unique identifier of the parent.
                /// </summary>
                GET_VIRTUAL_DISK_INFO_PARENT_IDENTIFIER = 4,

                /// <summary>
                /// Time stamp of the parent.
                /// </summary>
                GET_VIRTUAL_DISK_INFO_PARENT_TIMESTAMP = 5,

                /// <summary>
                /// Type.
                /// </summary>
                GET_VIRTUAL_DISK_INFO_VIRTUAL_STORAGE_TYPE = 6,

                /// <summary>
                /// Subtype.
                /// </summary>
                GET_VIRTUAL_DISK_INFO_PROVIDER_SUBTYPE = 7
            }

            /// <summary>
            /// Contains virtual hard disk (VHD) merge request flags.
            /// </summary>
            public enum MERGE_VIRTUAL_DISK_FLAG : int {
                /// <summary>
                /// </summary>
                MERGE_VIRTUAL_DISK_FLAG_NONE = 0x00000000
            }

            /// <summary>
            /// Contains the version of the virtual hard disk (VHD) MERGE_VIRTUAL_DISK_PARAMETERS structure to use in calls to VHD functions.
            /// </summary>
            public enum MERGE_VIRTUAL_DISK_VERSION : int {
                /// <summary>
                /// </summary>
                MERGE_VIRTUAL_DISK_VERSION_UNSPECIFIED = 0,

                /// <summary>
                /// </summary>
                MERGE_VIRTUAL_DISK_VERSION_1 = 1
            }

            /// <summary>
            /// Contains virtual disk open request flags.
            /// </summary>
            public enum OPEN_VIRTUAL_DISK_FLAG : int {
                /// <summary>
                /// No flag specified.
                /// </summary>
                OPEN_VIRTUAL_DISK_FLAG_NONE = 0x00000000,

                /// <summary>
                /// Open the backing store without opening any differencing-chain parents. Used to correct broken parent links.
                /// </summary>
                OPEN_VIRTUAL_DISK_FLAG_NO_PARENTS = 0x00000001,

                /// <summary>
                /// Reserved.
                /// </summary>
                OPEN_VIRTUAL_DISK_FLAG_BLANK_FILE = 0x00000002,

                /// <summary>
                /// Reserved.
                /// </summary>
                OPEN_VIRTUAL_DISK_FLAG_BOOT_DRIVE = 0x00000004
            }

            /// <summary>
            /// Contains the version of the virtual hard disk (VHD) OPEN_VIRTUAL_DISK_PARAMETERS structure to use in calls to VHD functions.
            /// </summary>
            public enum OPEN_VIRTUAL_DISK_VERSION : int {
                /// <summary>
                /// </summary>
                OPEN_VIRTUAL_DISK_VERSION_UNSPECIFIED = 0,

                /// <summary>
                /// </summary>
                OPEN_VIRTUAL_DISK_VERSION_1 = 1
            }

            /// <summary>
            /// Contains the version of the virtual hard disk (VHD) SET_VIRTUAL_DISK_INFO structure to use in calls to VHD functions.
            /// </summary>
            public enum SET_VIRTUAL_DISK_INFO_VERSION : int {
                /// <summary>
                /// Not used. Will fail the operation.
                /// </summary>
                SET_VIRTUAL_DISK_INFO_UNSPECIFIED = 0,

                /// <summary>
                /// Parent information is being set.
                /// </summary>
                SET_VIRTUAL_DISK_INFO_PARENT_PATH = 1,

                /// <summary>
                /// A unique identifier is being set. 
                /// </summary>
                SET_VIRTUAL_DISK_INFO_IDENTIFIER = 2
            }

            /// <summary>
            /// Contains the version of the virtual hard disk (VHD) STORAGE_DEPENDENCY_INFO structure to use in calls to VHD functions.
            /// </summary>
            public enum STORAGE_DEPENDENCY_INFO_VERSION : int {
                /// <summary>
                /// The version is not specified.
                /// </summary>
                STORAGE_DEPENDENCY_INFO_VERSION_UNSPECIFIED = 0,

                /// <summary>
                /// Specifies STORAGE_DEPENDENCY_INFO_TYPE_1.
                /// </summary>
                STORAGE_DEPENDENCY_INFO_VERSION_1 = 1,

                /// <summary>
                /// Specifies STORAGE_DEPENDENCY_INFO_TYPE_2.
                /// </summary>
                STORAGE_DEPENDENCY_INFO_VERSION_2 = 2
            }

            /// <summary>
            /// Contains the bit mask for specifying access rights to a virtual hard disk (VHD).
            /// </summary>
            public enum VIRTUAL_DISK_ACCESS_MASK : int {
                /// <summary>
                /// Open the virtual disk for read-only attach access. The caller must have READ access to the virtual disk image file. If used in a request to open a virtual disk that is already open, the other handles must be limited to either VIRTUAL_DISK_ACCESS_DETACH or VIRTUAL_DISK_ACCESS_GET_INFO access, otherwise the open request with this flag will fail.
                /// </summary>
                VIRTUAL_DISK_ACCESS_ATTACH_RO = 0x00010000,

                /// <summary>
                /// Open the virtual disk for read-write attaching access. The caller must have (READ | WRITE) access to the virtual disk image file. If used in a request to open a virtual disk that is already open, the other handles must be limited to either VIRTUAL_DISK_ACCESS_DETACH or VIRTUAL_DISK_ACCESS_GET_INFO access, otherwise the open request with this flag will fail. If the virtual disk is part of a differencing chain, the disk for this request cannot be less than the RWDepth specified during the prior open request for that differencing chain.
                /// </summary>
                VIRTUAL_DISK_ACCESS_ATTACH_RW = 0x00020000,

                /// <summary>
                /// Open the virtual disk to allow detaching of an attached virtual disk. The caller must have (FILE_READ_ATTRIBUTES | FILE_READ_DATA) access to the virtual disk image file.
                /// </summary>
                VIRTUAL_DISK_ACCESS_DETACH = 0x00040000,

                /// <summary>
                /// Information retrieval access to the VHD. The caller must have READ access to the virtual disk image file.
                /// </summary>
                VIRTUAL_DISK_ACCESS_GET_INFO = 0x00080000,

                /// <summary>
                /// VHD creation access.
                /// </summary>
                VIRTUAL_DISK_ACCESS_CREATE = 0x00100000,

                /// <summary>
                /// Open the VHD to perform offline meta-operations. The caller must have (READ | WRITE) access to the virtual disk image file, up to RWDepth if working with a differencing chain. If the VHD is part of a differencing chain, the backing store (host volume) is opened in RW exclusive mode up to RWDepth.
                /// </summary>
                VIRTUAL_DISK_ACCESS_METAOPS = 0x00200000,

                /// <summary>
                /// Reserved.
                /// </summary>
                VIRTUAL_DISK_ACCESS_READ = 0x000d0000,

                /// <summary>
                /// Allows unrestricted access to the VHD. The caller must have unrestricted access rights to the virtual disk image file.
                /// </summary>
                VIRTUAL_DISK_ACCESS_ALL = 0x003f0000,

                /// <summary>
                /// Reserved.
                /// </summary>
                VIRTUAL_DISK_ACCESS_WRITABLE = 0x00320000
            }

            #endregion


            #region VHD Structures

            /// <summary>
            /// Contains virtual hard disk (VHD) attach request parameters.
            /// </summary>
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            public struct ATTACH_VIRTUAL_DISK_PARAMETERS {
                /// <summary>
                /// A SURFACE_VIRTUAL_DISK_VERSION enumeration that specifies the version of the SURFACE_VIRTUAL_DISK_PARAMETERS structure being passed to or from the VHD functions.
                /// </summary>
                public ATTACH_VIRTUAL_DISK_VERSION Version; //SURFACE_VIRTUAL_DISK_VERSION

                /// <summary>
                /// A structure.
                /// </summary>
                public ATTACH_VIRTUAL_DISK_PARAMETERS_Version1 Version1;
            }

            /// <summary>
            /// A structure.
            /// </summary>
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            public struct ATTACH_VIRTUAL_DISK_PARAMETERS_Version1 {
                /// <summary>
                /// Reserved.
                /// </summary>
                public Int32 Reserved; //ULONG
            }

            /// <summary>
            /// Contains virtual disk compacting parameters.
            /// </summary>
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            public struct COMPACT_VIRTUAL_DISK_PARAMETERS {
                /// <summary>
                /// A COMPACT_VIRTUAL_DISK_VERSION enumeration that specifies the version of the COMPACT_VIRTUAL_DISK_PARAMETERS structure being passed to or from the virtual hard disk (VHD) functions.
                /// </summary>
                public COMPACT_VIRTUAL_DISK_VERSION Version; //COMPACT_VIRTUAL_DISK_VERSION

                /// <summary>
                /// </summary>
                public COMPACT_VIRTUAL_DISK_PARAMETERS_Version1 Version1;
            }

            /// <summary>
            /// A structure.
            /// </summary>
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            public struct COMPACT_VIRTUAL_DISK_PARAMETERS_Version1 {
                /// <summary>
                /// Reserved. Must be set to zero.
                /// </summary>
                public Int32 Reserved; //ULONG
            }


            /// <summary>
            /// Contains virtual disk creation parameters, providing control over, and information about, the newly created virtual disk.
            /// </summary>
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            public struct CREATE_VIRTUAL_DISK_PARAMETERS {
                /// <summary>
                /// A CREATE_VIRTUAL_DISK_VERSION enumeration that specifies the version of the CREATE_VIRTUAL_DISK_PARAMETERS structure being passed to or from the virtual hard disk (VHD) functions.
                /// </summary>
                public CREATE_VIRTUAL_DISK_VERSION Version; //CREATE_VIRTUAL_DISK_VERSION

                /// <summary>
                /// A structure.
                /// </summary>
                public CREATE_VIRTUAL_DISK_PARAMETERS_Version1 Version1;
            }

            /// <summary>
            /// </summary>
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            public struct CREATE_VIRTUAL_DISK_PARAMETERS_Version1 {
                /// <summary>
                /// Unique identifier to assign to the virtual disk object. If this member is set to zero, a unique identifier is created by the system.
                /// </summary>
                public Guid UniqueId; //GUID

                /// <summary>
                /// The maximum virtual size of the virtual disk object. Must be a multiple of 512. If a ParentPath is specified, this value must be zero. If a SourcePath is specified, this value can be zero to specify the size of the source VHD to be used, otherwise the size specified must be greater than or equal to the size of the source disk.
                /// </summary>
                public Int64 MaximumSize; //ULONGLONG

                /// <summary>
                /// Internal size of the virtual disk object blocks. If value is 0, block size will be automatically matched to the parent or source disk's setting if ParentPath or SourcePath is specified (otherwise a block size of 2MB will be used).
                /// </summary>
                public Int32 BlockSizeInBytes; //ULONG

                /// <summary>
                /// Internal size of the virtual disk object sectors. Must be set to 512.
                /// </summary>
                public Int32 SectorSizeInBytes; //ULONG

                /// <summary>
                /// Optional path to a parent virtual disk object. Associates the new virtual disk with an existing virtual disk. If this parameter is not NULL, SourcePath must be NULL.
                /// </summary>
                public IntPtr ParentPath; //PCWSTR

                /// <summary>
                /// Optional path to pre-populate the new virtual disk object with block data from an existing disk. This path may refer to a VHD or a physical disk. If this parameter is not NULL, ParentPath must be NULL.
                /// </summary>
                public IntPtr SourcePath; //PCWSTR
            }


            /// <summary>
            /// Contains virtual disk expansion request parameters.
            /// </summary>
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            public struct EXPAND_VIRTUAL_DISK_PARAMETERS {
                /// <summary>
                /// An EXPAND_VIRTUAL_DISK_VERSION enumeration that specifies the version of the EXPAND_VIRTUAL_DISK_PARAMETERS structure being passed to or from the virtual hard disk (VHD) functions.
                /// </summary>
                public EXPAND_VIRTUAL_DISK_VERSION Version; //EXPAND_VIRTUAL_DISK_VERSION

                /// <summary>
                /// </summary>
                public EXPAND_VIRTUAL_DISK_PARAMETERS_Version1 Version1;
            }

            /// <summary>
            /// A structure.
            /// </summary>
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            public struct EXPAND_VIRTUAL_DISK_PARAMETERS_Version1 {
                /// <summary>
                /// New size, in bytes, for the expansion request.
                /// </summary>
                public Int64 NewSize; //ULONGLONG
            }


            /// <summary>
            /// Contains virtual hard disk (VHD) information.
            /// </summary>
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            public struct GET_VIRTUAL_DISK_INFO {
                /// <summary>
                /// A GET_VIRTUAL_DISK_INFO_VERSION enumeration that specifies the version of the GET_VIRTUAL_DISK_INFO structure being passed to or from the VHD functions. This determines what parts of this structure will be used.
                /// </summary>
                public GET_VIRTUAL_DISK_INFO_VERSION Version; //GET_VIRTUAL_DISK_INFO_VERSION

                /// <summary>
                /// </summary>
                public GET_VIRTUAL_DISK_INFO_Union Union;
            }

            /// <summary>
            /// </summary>
            [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode)]
            public struct GET_VIRTUAL_DISK_INFO_Union {
                /// <summary>
                /// </summary>
                [FieldOffset(0)]
                public GET_VIRTUAL_DISK_INFO_Size Size;

                /// <summary>
                /// Unique identifier of the VHD.
                /// </summary>
                [FieldOffset(0)]
                public Guid Identifier;

                /// <summary>
                /// </summary>
                [FieldOffset(0)]
                public GET_VIRTUAL_DISK_INFO_ParentLocation ParentLocation;

                /// <summary>
                /// Unique identifier of the parent disk backing store.
                /// </summary>
                [FieldOffset(0)]
                public Guid ParentIdentifier;

                /// <summary>
                /// Internal time stamp of the parent disk backing store.
                /// </summary>
                [FieldOffset(0)]
                public Int32 ParentTimestamp; //ULONG

                /// <summary>
                /// VIRTUAL_STORAGE_TYPE structure containing information about the type of VHD.
                /// </summary>
                [FieldOffset(0)]
                public VIRTUAL_STORAGE_TYPE VirtualStorageType; //VIRTUAL_STORAGE_TYPE

                /// <summary>
                /// Provider-specific subtype.
                /// </summary>
                [FieldOffset(0)]
                public Int32 ProviderSubtype; //ULONG
            }

            /// <summary>
            /// A structure.
            /// </summary>
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            public struct GET_VIRTUAL_DISK_INFO_Size {
                /// <summary>
                /// Virtual size of the VHD, in bytes.
                /// </summary>
                Int64 VirtualSize; //ULONGLONG

                /// <summary>
                /// Physical size of the VHD on disk, in bytes.
                /// </summary>
                Int64 PhysicalSize; //ULONGLONG

                /// <summary>
                /// Block size of the VHD, in bytes.
                /// </summary>
                Int32 BlockSize; //ULONG

                /// <summary>
                /// Sector size of the VHD, in bytes.
                /// </summary>
                Int32 SectorSize; //ULONG
            }

            /// <summary>
            /// </summary>
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            public struct GET_VIRTUAL_DISK_INFO_ParentLocation {
                /// <summary>
                /// Parent resolution. TRUE if the parent backing store was successfully resolved, FALSE if not.
                /// </summary>
                [MarshalAsAttribute(UnmanagedType.Bool)]
                public bool ParentResolved; //BOOL

                /// <summary>
                /// If the ParentResolved member is TRUE, contains the path of the parent backing store. If the ParentResolved member is FALSE, contains all of the parent paths present in the search list.
                /// </summary>
                public char ParentLocationBuffer; //WCHAR[1] //TODO
            }



            /// <summary>
            /// Contains virtual disk merge request parameters.
            /// </summary>
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            public struct MERGE_VIRTUAL_DISK_PARAMETERS {
                /// <summary>
                /// A MERGE_VIRTUAL_DISK_VERSION enumeration that specifies the version of the MERGE_VIRTUAL_DISK_PARAMETERS structure being passed to or from the VHD functions.
                /// </summary>
                public MERGE_VIRTUAL_DISK_VERSION Version; //MERGE_VIRTUAL_DISK_VERSION

                /// <summary>
                /// </summary>
                public MERGE_VIRTUAL_DISK_PARAMETERS_Version1 Version1;
            }

            /// <summary>
            /// A structure.
            /// </summary>
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            public struct MERGE_VIRTUAL_DISK_PARAMETERS_Version1 {
                /// <summary>
                /// Depth of the merge request. This is the number of parent disks in the differencing chain to merge together. Note that RWDepth of the VHD must be set to equal to or greater than this value.
                /// </summary>
                public Int32 RWDepth; //ULONG
            }


            /// <summary>
            /// Contains virtual disk open request parameters.
            /// </summary>
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            public struct OPEN_VIRTUAL_DISK_PARAMETERS {
                /// <summary>
                /// An OPEN_VIRTUAL_DISK_VERSION enumeration that specifies the version of the OPEN_VIRTUAL_DISK_PARAMETERS structure being passed to or from the VHD functions.
                /// </summary>
                public OPEN_VIRTUAL_DISK_VERSION Version; //OPEN_VIRTUAL_DISK_VERSION

                /// <summary>
                /// A structure.
                /// </summary>
                public OPEN_VIRTUAL_DISK_PARAMETERS_Version1 Version1;
            }

            /// <summary>
            /// </summary>
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            public struct OPEN_VIRTUAL_DISK_PARAMETERS_Version1 {
                /// <summary>
                /// Indicates the number of stores, beginning with the child, of the backing store chain to open as read/write. The remaining stores in the differencing chain will be opened read-only. This is necessary for merge operations to succeed.
                /// </summary>
                public Int32 RWDepth; //ULONG
            }


            /// <summary>
            /// Contains virtual hard disk (VHD) information for set request.
            /// </summary>
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            public struct SET_VIRTUAL_DISK_INFO {
                /// <summary>
                /// A SET_VIRTUAL_DISK_INFO_VERSION enumeration that specifies the version of the SET_VIRTUAL_DISK_INFO structure being passed to or from the VHD functions. This determines the type of information set.
                /// </summary>
                public SET_VIRTUAL_DISK_INFO_VERSION Version;

                /// <summary>
                /// </summary>
                public SET_VIRTUAL_DISK_INFO_Union Union;
            }

            /// <summary>
            /// </summary>
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            public struct SET_VIRTUAL_DISK_INFO_Union {
                /// <summary>
                /// Path to the parent backing store.
                /// </summary>
                public String ParentFilePath; //PCWSTR

                /// <summary>
                /// Unique identifier of the VHD.
                /// </summary>
                public Guid UniqueIdentifier; //GUID
            }


            /// <summary>
            /// Contains storage dependency information.
            /// </summary>
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            public struct STORAGE_DEPENDENCY_INFO {
                /// <summary>
                /// A STORAGE_DEPENDENCY_INFO_VERSION enumeration that specifies the version of the information structure being passed to or from the VHD functions. Can be STORAGE_DEPENDENCY_INFO_TYPE_1 or STORAGE_DEPENDENCY_INFO_TYPE_2.
                /// </summary>
                public STORAGE_DEPENDENCY_INFO_VERSION Version;

                /// <summary>
                /// Number of entries returned in the following unioned members.
                /// </summary>
                public Int32 NumberEntries; //ULONG

                /// <summary>
                /// </summary>
                public STORAGE_DEPENDENCY_INFO_Union Union;
            }

            /// <summary>
            /// </summary>
            [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode)]
            public struct STORAGE_DEPENDENCY_INFO_Union {
                /// <summary>
                /// Variable-length array containing STORAGE_DEPENDENCY_INFO_TYPE_1 structures.
                /// </summary>
                [FieldOffset(0)]
                public STORAGE_DEPENDENCY_INFO_TYPE_1 Version1Entries; //STORAGE_DEPENDENCY_INFO_TYPE_1[]

                /// <summary>
                /// Variable-length array containing STORAGE_DEPENDENCY_INFO_TYPE_2 structures.
                /// </summary>
                [FieldOffset(0)]
                public STORAGE_DEPENDENCY_INFO_TYPE_2 Version2Entries; //STORAGE_DEPENDENCY_INFO_TYPE_2[]
            }


            /// <summary>
            /// Contains storage dependency information for type 1.
            /// </summary>
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            public struct STORAGE_DEPENDENCY_INFO_TYPE_1 {
                /// <summary>
                /// A DEPENDENT_DISK_FLAG enumeration.
                /// </summary>
                public DEPENDENT_DISK_FLAG DependencyTypeFlags; //DEPENDENT_DISK_FLAG 

                /// <summary>
                /// Flags specific to the VHD provider.
                /// </summary>
                public Int32 ProviderSpecificFlags; //ULONG

                /// <summary>
                /// A VIRTUAL_STORAGE_TYPE structure.
                /// </summary>
                public VIRTUAL_STORAGE_TYPE VirtualStorageType; //VIRTUAL_STORAGE_TYPE
            }


            /// <summary>
            /// Contains storage dependency information for type 2.
            /// </summary>
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            public struct STORAGE_DEPENDENCY_INFO_TYPE_2 {
                /// <summary>
                /// A DEPENDENT_DISK_FLAG enumeration.
                /// </summary>
                public DEPENDENT_DISK_FLAG DependencyTypeFlags; //DEPENDENT_DISK_FLAG

                /// <summary>
                /// Flags specific to the VHD provider.
                /// </summary>
                public Int32 ProviderSpecificFlags; //ULONG

                /// <summary>
                /// A VIRTUAL_STORAGE_TYPE structure.
                /// </summary>
                public VIRTUAL_STORAGE_TYPE VirtualStorageType; //VIRTUAL_STORAGE_TYPE

                /// <summary>
                /// The ancestor level.
                /// </summary>
                public Int32 AncestorLevel; //ULONG

                /// <summary>
                /// The device name of the dependent device.
                /// </summary>
                [MarshalAs(UnmanagedType.LPWStr)]
                public String DependencyDeviceName; //PWSTR 

                /// <summary>
                /// The host disk volume name.
                /// </summary>
                public IntPtr HostVolumeName; //PWSTR

                /// <summary>
                /// The name of the dependent volume, if any.
                /// </summary>
                public IntPtr DependentVolumeName; //PWSTR

                /// <summary>
                /// The relative path to the dependent volume.
                /// </summary>
                public IntPtr DependentVolumeRelativePath; //PWSTR
            }




            /// <summary>
            /// Contains the progress and result data for the current virtual disk operation, used by the GetVirtualDiskOperationProgress function.
            /// </summary>
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            public struct VIRTUAL_DISK_PROGRESS {
                /// <summary>
                /// A system error code status value, this member will be ERROR_IO_PENDING if the operation is still in progress; otherwise, the value is the result code of the completed operation.
                /// </summary>
                public Int32 OperationStatus; //DWORD

                /// <summary>
                /// The current progress of the operation, used in conjunction with the CompletionValue member. This value is meaningful only if OperationStatus is ERROR_IO_PENDING.
                /// </summary>
                public Int64 CurrentValue; //ULONGLONG

                /// <summary>
                /// The value that the CurrentValue member would be if the operation were complete. This value is meaningful only if OperationStatus is ERROR_IO_PENDING.
                /// </summary>
                public Int64 CompletionValue; //ULONGLONG
            }

            /// <summary>
            /// Contains the type and provider (vendor) of the virtual storage device.
            /// </summary>
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            public struct VIRTUAL_STORAGE_TYPE {
                /// <summary>
                /// Device type identifier.
                /// </summary>
                public Int32 DeviceId; //ULONG

                /// <summary>
                /// Vendor-unique identifier.
                /// </summary>
                public Guid VendorId; //GUID
            }

            #endregion


            #region VHD Functions

            /// <summary>
            /// Attaches a virtual hard disk (VHD) by locating an appropriate VHD provider to accomplish the attachment.
            /// </summary>
            /// <param name="VirtualDiskHandle">A handle to an open VHD.</param>
            /// <param name="SecurityDescriptor">An optional pointer to a SECURITY_DESCRIPTOR to apply to the attached virtual disk. If this parameter is NULL, the security descriptor of the virtual disk image file will be used.</param>
            /// <param name="Flags">A valid combination of values of the SURFACE_VIRTUAL_DISK_FLAG enumeration.</param>
            /// <param name="ProviderSpecificFlags">Flags specific to the type of virtual disk being attached. May be zero if none are required.</param>
            /// <param name="Parameters">A pointer to a valid SURFACE_VIRTUAL_DISK_PARAMETERS structure that contains surfacing (attachment) parameter data.</param>
            /// <param name="Overlapped">An optional pointer to a valid OVERLAPPED structure if asynchronous operation is desired.</param>
            /// <returns>If the function succeeds, the return value is ERROR_SUCCESS and the Handle parameter contains a valid pointer to the new virtual disk object. If the function fails, the return value is an error code and the value of the Handle parameter is undefined.</returns>
            [DllImport("virtdisk.dll", CharSet = CharSet.Unicode)]
            public static extern Int32 AttachVirtualDisk(VirtualDiskSafeHandle VirtualDiskHandle, IntPtr SecurityDescriptor, ATTACH_VIRTUAL_DISK_FLAG Flags, Int32 ProviderSpecificFlags, ref ATTACH_VIRTUAL_DISK_PARAMETERS Parameters, IntPtr Overlapped);

            /// <summary>
            /// Reduces the size of a virtual hard disk (VHD) backing store file.
            /// </summary>
            /// <param name="VirtualDiskHandle">A handle to the open VHD, which must have been opened using the VIRTUAL_DISK_ACCESS_METAOPS flag.</param>
            /// <param name="Flags">Must be the COMPACT_VIRTUAL_DISK_FLAG_NONE value of the COMPACT_VIRTUAL_DISK_FLAG enumeration.</param>
            /// <param name="Parameters">A pointer to a valid COMPACT_VIRTUAL_DISK_PARAMETERS structure that contains compaction parameter data.</param>
            /// <param name="Overlapped">An optional pointer to a valid OVERLAPPED structure if asynchronous operation is desired.</param>
            /// <returns>If the function succeeds, the return value is ERROR_SUCCESS and the Handle parameter contains a valid pointer to the new virtual disk object. If the function fails, the return value is an error code and the value of the Handle parameter is undefined.</returns>
            [DllImport("virtdisk.dll", CharSet = CharSet.Unicode)]
            public static extern Int32 CompactVirtualDisk(VirtualDiskSafeHandle VirtualDiskHandle, COMPACT_VIRTUAL_DISK_FLAG Flags, ref COMPACT_VIRTUAL_DISK_PARAMETERS Parameters, IntPtr Overlapped);

            /// <summary>
            /// Creates a virtual hard disk (VHD) image file, either using default parameters or using an existing VHD or physical disk.
            /// </summary>
            /// <param name="VirtualStorageType">A pointer to a VIRTUAL_STORAGE_TYPE structure that contains the desired disk type and vendor information.</param>
            /// <param name="Path">A pointer to a valid string that represents the path to the new virtual disk image file.</param>
            /// <param name="VirtualDiskAccessMask">The VIRTUAL_DISK_ACCESS_MASK value to use when opening the newly created virtual disk file.</param>
            /// <param name="SecurityDescriptor">An optional pointer to a SECURITY_DESCRIPTOR to apply to the virtual disk image file. If this parameter is NULL, the parent directory's security descriptor will be used.</param>
            /// <param name="Flags">Creation flags, which must be a valid combination of the CREATE_VIRTUAL_DISK_FLAG enumeration.</param>
            /// <param name="ProviderSpecificFlags">Flags specific to the type of virtual disk being created. May be zero if none are required.</param>
            /// <param name="Parameters">A pointer to a valid CREATE_VIRTUAL_DISK_PARAMETERS structure that contains creation parameter data.</param>
            /// <param name="Overlapped">An optional pointer to a valid OVERLAPPED structure if asynchronous operation is desired.</param>
            /// <param name="Handle">A pointer to the handle object that represents the newly created virtual disk.</param>
            /// <returns>If the function succeeds, the return value is ERROR_SUCCESS and the Handle parameter contains a valid pointer to the new virtual disk object. If the function fails, the return value is an error code and the value of the Handle parameter is undefined.</returns>
            [DllImport("virtdisk.dll", CharSet = CharSet.Unicode)]
            public static extern Int32 CreateVirtualDisk(ref VIRTUAL_STORAGE_TYPE VirtualStorageType, String Path, VIRTUAL_DISK_ACCESS_MASK VirtualDiskAccessMask, IntPtr SecurityDescriptor, CREATE_VIRTUAL_DISK_FLAG Flags, Int32 ProviderSpecificFlags, ref CREATE_VIRTUAL_DISK_PARAMETERS Parameters, IntPtr Overlapped, ref VirtualDiskSafeHandle Handle);

            /// <summary>
            /// Creates a virtual hard disk (VHD) image file, either using default parameters or using an existing VHD or physical disk.
            /// </summary>
            /// <param name="VirtualStorageType">A pointer to a VIRTUAL_STORAGE_TYPE structure that contains the desired disk type and vendor information.</param>
            /// <param name="Path">A pointer to a valid string that represents the path to the new virtual disk image file.</param>
            /// <param name="VirtualDiskAccessMask">The VIRTUAL_DISK_ACCESS_MASK value to use when opening the newly created virtual disk file.</param>
            /// <param name="SecurityDescriptor">An optional pointer to a SECURITY_DESCRIPTOR to apply to the virtual disk image file. If this parameter is NULL, the parent directory's security descriptor will be used.</param>
            /// <param name="Flags">Creation flags, which must be a valid combination of the CREATE_VIRTUAL_DISK_FLAG enumeration.</param>
            /// <param name="ProviderSpecificFlags">Flags specific to the type of virtual disk being created. May be zero if none are required.</param>
            /// <param name="Parameters">A pointer to a valid CREATE_VIRTUAL_DISK_PARAMETERS structure that contains creation parameter data.</param>
            /// <param name="Overlapped">An optional pointer to a valid OVERLAPPED structure if asynchronous operation is desired.</param>
            /// <param name="Handle">A pointer to the handle object that represents the newly created virtual disk.</param>
            /// <returns>If the function succeeds, the return value is ERROR_SUCCESS and the Handle parameter contains a valid pointer to the new virtual disk object. If the function fails, the return value is an error code and the value of the Handle parameter is undefined.</returns>
            [DllImport("virtdisk.dll", CharSet = CharSet.Unicode)]
            public static extern Int32 CreateVirtualDisk(ref VIRTUAL_STORAGE_TYPE VirtualStorageType, String Path, VIRTUAL_DISK_ACCESS_MASK VirtualDiskAccessMask, IntPtr SecurityDescriptor, CREATE_VIRTUAL_DISK_FLAG Flags, Int32 ProviderSpecificFlags, ref CREATE_VIRTUAL_DISK_PARAMETERS Parameters, ref NativeOverlapped Overlapped, ref VirtualDiskSafeHandle Handle);

            /// <summary>
            /// Detaches a virtual hard disk (VHD) by locating an appropriate VHD provider to accomplish the operation.
            /// </summary>
            /// <param name="VirtualDiskHandle">A handle to an open VHD, which must have been opened using the VIRTUAL_DISK_ACCESS_UNSURFACE flag.</param>
            /// <param name="Flags">A valid combination of values of the UNSURFACE_VIRTUAL_DISK_FLAG enumeration.</param>
            /// <param name="ProviderSpecificFlags">Flags specific to the type of virtual disk being unsurfaced. May be zero if none are required.</param>
            /// <returns>If the function succeeds, the return value is ERROR_SUCCESS and the Handle parameter contains a valid pointer to the new virtual disk object. If the function fails, the return value is an error code and the value of the Handle parameter is undefined.</returns>
            [DllImport("virtdisk.dll", CharSet = CharSet.Unicode)]
            public static extern Int32 DetachVirtualDisk(VirtualDiskSafeHandle VirtualDiskHandle, DETACH_VIRTUAL_DISK_FLAG Flags, Int32 ProviderSpecificFlags);

            /// <summary>
            /// Increases the size of a fixed or dynamic virtual hard disk (VHD).
            /// </summary>
            /// <param name="VirtualDiskHandle">A handle to the open VHD, which must have been opened using the VIRTUAL_DISK_ACCESS_METAOPS flag.</param>
            /// <param name="Flags">Must be the EXPAND_VIRTUAL_DISK_FLAG_NONE value of the EXPAND_VIRTUAL_DISK_FLAG enumeration.</param>
            /// <param name="Parameters">A pointer to a valid EXPAND_VIRTUAL_DISK_PARAMETERS structure that contains expansion parameter data.</param>
            /// <param name="Overlapped">An optional pointer to a valid OVERLAPPED structure if asynchronous operation is desired.</param>
            /// <returns>If the function succeeds, the return value is ERROR_SUCCESS and the Handle parameter contains a valid pointer to the new virtual disk object. If the function fails, the return value is an error code and the value of the Handle parameter is undefined.</returns>
            [DllImport("virtdisk.dll", CharSet = CharSet.Unicode)]
            public static extern Int32 ExpandVirtualDisk(VirtualDiskSafeHandle VirtualDiskHandle, EXPAND_VIRTUAL_DISK_FLAG Flags, ref EXPAND_VIRTUAL_DISK_PARAMETERS Parameters, IntPtr Overlapped);

            /// <summary>
            /// Returns the relationships between virtual hard disks (VHDs) or the volumes contained within those disks and their parent disk or volume.
            /// </summary>
            /// <param name="ObjectHandle">A handle to an open VHD.</param>
            /// <param name="Flags">A valid combination of GET_STORAGE_DEPENDENCY_FLAG values.</param>
            /// <param name="StorageDependencyInfoSize">Size, in bytes, of the STORAGE_DEPENDENCY_INFO structure that the StorageDependencyInfo parameter refers to.</param>
            /// <param name="StorageDependencyInfo">A pointer to a valid STORAGE_DEPENDENCY_INFO structure, which is a variable-length structure.</param>
            /// <param name="SizeUsed">An optional pointer to a ULONG that receives the size used.</param>
            /// <returns>If the function succeeds, the return value is ERROR_SUCCESS and the Handle parameter contains a valid pointer to the new virtual disk object. If the function fails, the return value is an error code and the value of the Handle parameter is undefined.</returns>
            [DllImport("virtdisk.dll", CharSet = CharSet.Unicode)]
            public static extern Int32 GetStorageDependencyInformation(VirtualDiskSafeHandle ObjectHandle, GET_STORAGE_DEPENDENCY_FLAG Flags, Int32 StorageDependencyInfoSize, ref STORAGE_DEPENDENCY_INFO StorageDependencyInfo, ref Int32 SizeUsed);

            /// <summary>
            /// Retrieves information about a virtual hard disk (VHD).
            /// </summary>
            /// <param name="VirtualDiskHandle">A handle to the open VHD, which must have been opened using the VIRTUAL_DISK_ACCESS_GET_INFO flag.</param>
            /// <param name="VirtualDiskInfoSize">A pointer to a ULONG that contains the size of the VirtualDiskInfo parameter.</param>
            /// <param name="VirtualDiskInfo">A pointer to a valid GET_VIRTUAL_DISK_INFO structure. The format of the data returned is dependent on the value passed in the Version member by the caller.</param>
            /// <param name="SizeUsed">A pointer to a ULONG that contains the size used.</param>
            /// <returns>If the function succeeds, the return value is ERROR_SUCCESS and the Handle parameter contains a valid pointer to the new virtual disk object. If the function fails, the return value is an error code and the value of the Handle parameter is undefined.</returns>
            [DllImport("virtdisk.dll", CharSet = CharSet.Unicode)]
            public static extern Int32 GetVirtualDiskInformation(VirtualDiskSafeHandle VirtualDiskHandle, ref Int32 VirtualDiskInfoSize, ref GET_VIRTUAL_DISK_INFO VirtualDiskInfo, ref Int32 SizeUsed);

            /// <summary>
            /// Checks the progress of an asynchronous virtual hard disk (VHD) operation.
            /// </summary>
            /// <param name="VirtualDiskHandle">A valid handle to a virtual disk with a pending asynchronous operation.</param>
            /// <param name="Overlapped">A pointer to a valid OVERLAPPED structure. This parameter must reference the same structure previously sent to the virtual disk operation being checked for progress.</param>
            /// <param name="Progress">A pointer to a VIRTUAL_DISK_PROGRESS structure that receives the current virtual disk operation progress.</param>
            /// <returns>If the function succeeds, the return value is ERROR_SUCCESS and the Handle parameter contains a valid pointer to the new virtual disk object. If the function fails, the return value is an error code and the value of the Handle parameter is undefined.</returns>
            [DllImport("virtdisk.dll", CharSet = CharSet.Unicode)]
            public static extern Int32 GetVirtualDiskOperationProgress(VirtualDiskSafeHandle VirtualDiskHandle, ref NativeOverlapped Overlapped, ref VIRTUAL_DISK_PROGRESS Progress);


            /// <summary>
            /// Retrieves the path to the physical device object that contains a virtual hard disk (VHD).
            /// </summary>
            /// <param name="VirtualDiskHandle">A handle to the open VHD, which must have been opened using the VIRTUAL_DISK_ACCESS_GET_INFO flag.</param>
            /// <param name="DiskPathSizeInBytes">The size, in bytes, of the buffer pointed to by the DiskPath parameter.</param>
            /// <param name="DiskPath">A target buffer to receive the path of the physical disk device that contains the VHD.</param>
            /// <returns>If the function succeeds, the return value is ERROR_SUCCESS and the Handle parameter contains a valid pointer to the new virtual disk object. If the function fails, the return value is an error code and the value of the Handle parameter is undefined.</returns>
            [DllImport("virtdisk.dll", CharSet = CharSet.Unicode)]
            public static extern Int32 GetVirtualDiskPhysicalPath(VirtualDiskSafeHandle VirtualDiskHandle, ref Int32 DiskPathSizeInBytes, StringBuilder DiskPath);

            /// <summary>
            /// Merges a child virtual hard disk (VHD) in a differencing chain with parent disks in the chain.
            /// </summary>
            /// <param name="VirtualDiskHandle">A handle to the open VHD, which must have been opened using the VIRTUAL_DISK_ACCESS_METAOPS flag.</param>
            /// <param name="Flags">Must be the MERGE_VIRTUAL_DISK_FLAG_NONE value of the MERGE_VIRTUAL_DISK_FLAG enumeration.</param>
            /// <param name="Parameters">A pointer to a valid MERGE_VIRTUAL_DISK_PARAMETERS structure that contains merge parameter data.</param>
            /// <param name="Overlapped">An optional pointer to a valid OVERLAPPED structure if asynchronous operation is desired.</param>
            /// <returns>If the function succeeds, the return value is ERROR_SUCCESS and the Handle parameter contains a valid pointer to the new virtual disk object. If the function fails, the return value is an error code and the value of the Handle parameter is undefined.</returns>
            [DllImport("virtdisk.dll", CharSet = CharSet.Unicode)]
            public static extern Int32 MergeVirtualDisk(VirtualDiskSafeHandle VirtualDiskHandle, MERGE_VIRTUAL_DISK_FLAG Flags, ref MERGE_VIRTUAL_DISK_PARAMETERS Parameters, IntPtr Overlapped);

            /// <summary>
            /// Opens a virtual hard disk (VHD) for use.
            /// </summary>
            /// <param name="VirtualStorageType">A pointer to a valid VIRTUAL_STORAGE_TYPE structure.</param>
            /// <param name="Path">A pointer to a valid path to the virtual disk image to open.</param>
            /// <param name="VirtualDiskAccessMask">A valid value of the VIRTUAL_DISK_ACCESS_MASK enumeration.</param>
            /// <param name="Flags">A valid combination of values of the OPEN_VIRTUAL_DISK_FLAG enumeration.</param>
            /// <param name="Parameters">An optional pointer to a valid OPEN_VIRTUAL_DISK_PARAMETERS structure. Can be NULL. </param>
            /// <param name="Handle">A pointer to the handle object that represents the open VHD.</param>
            /// <returns>If the function succeeds, the return value is ERROR_SUCCESS and the Handle parameter contains a valid pointer to the new virtual disk object. If the function fails, the return value is an error code and the value of the Handle parameter is undefined.</returns>
            [DllImport("virtdisk.dll", CharSet = CharSet.Unicode)]
            public static extern Int32 OpenVirtualDisk(ref VIRTUAL_STORAGE_TYPE VirtualStorageType, String Path, VIRTUAL_DISK_ACCESS_MASK VirtualDiskAccessMask, OPEN_VIRTUAL_DISK_FLAG Flags, ref OPEN_VIRTUAL_DISK_PARAMETERS Parameters, ref VirtualDiskSafeHandle Handle);

            /// <summary>
            /// Opens a virtual hard disk (VHD) for use.
            /// </summary>
            /// <param name="VirtualStorageType">A pointer to a valid VIRTUAL_STORAGE_TYPE structure.</param>
            /// <param name="Path">A pointer to a valid path to the virtual disk image to open.</param>
            /// <param name="VirtualDiskAccessMask">A valid value of the VIRTUAL_DISK_ACCESS_MASK enumeration.</param>
            /// <param name="Flags">A valid combination of values of the OPEN_VIRTUAL_DISK_FLAG enumeration.</param>
            /// <param name="Parameters">An optional pointer to a valid OPEN_VIRTUAL_DISK_PARAMETERS structure. Can be NULL. </param>
            /// <param name="Handle">A pointer to the handle object that represents the open VHD.</param>
            /// <returns>If the function succeeds, the return value is ERROR_SUCCESS and the Handle parameter contains a valid pointer to the new virtual disk object. If the function fails, the return value is an error code and the value of the Handle parameter is undefined.</returns>
            [DllImport("virtdisk.dll", CharSet = CharSet.Unicode)]
            public static extern Int32 OpenVirtualDisk(ref VIRTUAL_STORAGE_TYPE VirtualStorageType, String Path, VIRTUAL_DISK_ACCESS_MASK VirtualDiskAccessMask, OPEN_VIRTUAL_DISK_FLAG Flags, IntPtr Parameters, ref VirtualDiskSafeHandle Handle);

            /// <summary>
            /// Sets information about a virtual hard disk (VHD).
            /// </summary>
            /// <param name="VirtualDiskHandle">A handle to the open VHD, which must have been opened using the VIRTUAL_DISK_ACCESS_METAOPS flag.</param>
            /// <param name="VirtualDiskInfo">A pointer to a valid SET_VIRTUAL_DISK_INFO structure.</param>
            /// <returns>If the function succeeds, the return value is ERROR_SUCCESS and the Handle parameter contains a valid pointer to the new virtual disk object. If the function fails, the return value is an error code and the value of the Handle parameter is undefined.</returns>
            [DllImport("virtdisk.dll", CharSet = CharSet.Unicode)]
            public static extern Int32 SetVirtualDiskInformation(VirtualDiskSafeHandle VirtualDiskHandle, ref SET_VIRTUAL_DISK_INFO VirtualDiskInfo);

            #endregion


            #region Other

            /// <summary>
            /// The operation completed successfully.
            /// </summary>
            public const Int32 ERROR_SUCCESS = 0;

            /// <summary>
            /// The system cannot find the file specified.
            /// </summary>
            public const Int32 ERROR_FILE_NOT_FOUND = 2;

            /// <summary>
            /// The system cannot find the path specified.
            /// </summary>
            public const Int32 ERROR_PATH_NOT_FOUND = 3;

            /// <summary>
            /// Access is denied.
            /// </summary>
            public const Int32 ERROR_ACCESS_DENIED = 5;

            /// <summary>
            /// The handle is invalid.
            /// </summary>
            public const Int32 ERROR_INVALID_HANDLE = 6;

            /// <summary>
            /// The specified network resource or device is no longer available.
            /// </summary>
            public const Int32 ERROR_DEV_NOT_EXIST = 55;
            /// <summary>
            /// The file exists.
            /// </summary>
            public const Int32 ERROR_FILE_EXISTS = 80;
            /// <summary>
            /// The parameter is incorrect.
            /// </summary>
            public const Int32 ERROR_INVALID_PARAMETER = 87;
            /// <summary>
            /// The data area passed to a system call is too small.
            /// </summary>
            public const Int32 ERROR_INSUFFICIENT_BUFFER = 122;

            /// <summary>
            /// Overlapped I/O operation is in progress.
            /// </summary>
            public const Int32 ERROR_IO_PENDING = 997;

            /// <summary>
            /// Element not found.
            /// </summary>
            public const Int32 ERROR_NOT_FOUND = 1168;

            /// <summary>
            /// A required privilege is not held by the client.
            /// </summary>
            public const Int32 ERROR_PRIVILEGE_NOT_HELD = 1314;

            /// <summary>
            /// The file or directory is corrupted and unreadable.
            /// </summary>
            public const Int32 ERROR_FILE_CORRUPT = 1392;


            /// <summary>
            /// Closes an open object handle.
            /// </summary>
            /// <param name="hObject">A valid handle to an open object.</param>
            /// <returns>If the function succeeds, the return value is nonzero. If the function fails, the return value is zero. To get extended error information, call GetLastError.</returns>
            [DllImportAttribute("kernel32.dll", SetLastError = true)]
            [return: MarshalAsAttribute(UnmanagedType.Bool)]
            public static extern Boolean CloseHandle(IntPtr hObject);

            #endregion


            #region SafeHandle

            [SecurityPermission(SecurityAction.Demand)]
            public class VirtualDiskSafeHandle : SafeHandle {

                public VirtualDiskSafeHandle()
                    : base(IntPtr.Zero, true) { }


                public override bool IsInvalid {
                    get { return (this.IsClosed) || (base.handle == IntPtr.Zero); }
                }

                protected override bool ReleaseHandle() {
                    return CloseHandle(this.handle);
                }

                public override string ToString() {
                    return this.handle.ToString();
                }

            }

            #endregion

        }

    }


    /// <summary>
    /// Contains virtual disk attach request flags.
    /// </summary>
    [Flags()]
    public enum VirtualDiskAttachOptions {
        /// <summary>
        /// No flags. Use system defaults.
        /// </summary>
        None = 0x00000000,
        /// <summary>
        /// Attach the virtual disk as read-only.
        /// </summary>
        ReadOnly = 0x00000001,
        /// <summary>
        /// Will cause all volumes on the attached virtual disk to be mounted without assigning drive letters to them.
        /// </summary>
        NoDriveLetter = 0x00000002,
        /// <summary>
        /// Will decouple the virtual disk lifetime from that of the VirtualDiskHandle. The virtual disk will be attached until the DetachVirtualDisk function is called, even if all open handles to the virtual disk are closed.
        /// </summary>
        PermanentLifetime = 0x00000004,
        /// <summary>
        /// Reserved.
        /// </summary>
        NoLocalHost = 0x00000008,
    }





    /// <summary>
    /// Options for create operations.
    /// </summary>
    [Flags()]
    public enum VirtualDiskCreateOptions {
        /// <summary>
        /// No additional options are set.
        /// </summary>
        None = 0x00000000,
        /// <summary>
        /// Pre-allocate all physical space necessary for the size of the virtual disk.
        /// </summary>
        FullPhysicalAllocation = 0x00000001,
    }





    /// <summary>
    /// Structure with progress of asynchronous VHD operation.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes", Justification = "Equals has no meaning for this structure.")]
    public struct VirtualDiskOperationProgress {

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="progressPercentage">Percentage of operation in progress.</param>
        /// <param name="isDone">True if operation is completed.</param>
        public VirtualDiskOperationProgress(int progressPercentage, bool isDone)
            : this() {
            this.ProgressPercentage = progressPercentage;
            this.IsDone = isDone;
        }

        /// <summary>
        /// Gets operation progress percentage.
        /// </summary>
        public int ProgressPercentage { get; private set; }

        /// <summary>
        /// Gets whether operation is completed.
        /// </summary>
        public bool IsDone { get; private set; }

    }

}
