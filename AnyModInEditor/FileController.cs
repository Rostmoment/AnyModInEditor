﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace AnyModInEditor
{
    public class FileController
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class OpenFileName
        {
            public int structSize = Marshal.SizeOf(typeof(OpenFileName));
            public IntPtr dlgOwner = IntPtr.Zero;
            public IntPtr instance = IntPtr.Zero;
            public string filter = "RTMT, CBLD, and BLD Files\0*.rtmt;*.cbld;*.bld\0All Files\0*.*\0";
            public string customFilter = null;
            public int maxCustFilter = 0;
            public int filterIndex = 1;
            public string file = null;
            public int maxFile = 256;
            public string fileTitle = null;
            public int maxFileTitle = 64;
            public string initialDir = null;
            public string title = null;
            public int flags = 0x00080000 | 0x00000008;
            public short fileOffset = 0;
            public short fileExtension = 0;
            public string defExt = "bld";
            public IntPtr custData = IntPtr.Zero;
            public IntPtr hook = IntPtr.Zero;
            public string templateName = null;
            public IntPtr reservedPtr = IntPtr.Zero;
            public int reservedInt = 0;
            public int flagsEx = 0;
        }

        [DllImport("comdlg32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool GetOpenFileName([In, Out] OpenFileName ofn);

        public static string OpenFile()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            OpenFileName ofn = new OpenFileName();
            ofn.filter = "RTMT, CBLD, and BLD Files\0*.rtmt;*.cbld;*.bld\0All Files\0*.*\0";
            ofn.file = new string(new char[256]);
            ofn.maxFile = ofn.file.Length;
            ofn.fileTitle = new string(new char[64]);
            ofn.maxFileTitle = ofn.fileTitle.Length;
            ofn.initialDir = "C:\\";
            ofn.title = "Open RTMT, CBLD, or BLD File";
            ofn.defExt = "bld";

            if (GetOpenFileName(ofn))
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                return ofn.file;
            }
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            return null;
        }
    }
}
