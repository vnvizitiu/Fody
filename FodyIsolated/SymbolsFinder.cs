using System.IO;
using Mono.Cecil.Cil;
using Mono.Cecil.Mdb;
using Mono.Cecil.Pdb;

public partial class InnerWeaver
{
    bool pdbFound;
    bool mdbFound;
    ISymbolReaderProvider debugReaderProvider;
    ISymbolWriterProvider debugWriterProvider;
    string mdbPath;
    string pdbPath;

    void GetSymbolProviders()
    {
        FindPdb();

        FindMdb();

        ChooseNewest();

        if (pdbFound)
        {
            debugReaderProvider = new PdbReaderProvider();
            debugWriterProvider = new PdbWriterProvider();
            return;
        }

        if (mdbFound)
        {
            debugReaderProvider = new MdbReaderProvider();
            debugWriterProvider = new MdbWriterProvider();
            return;
        }

        throw new WeavingException("Found no debug symbols.");
    }

    void ChooseNewest()
    {
        if (!pdbFound || !mdbFound)
        {
            return;
        }
        if (File.GetLastWriteTimeUtc(pdbPath) >= File.GetLastWriteTimeUtc(mdbPath))
        {
            mdbFound = false;
            Logger.LogDebug("Found mdb and pdb debug symbols. Selected pdb (newer).");
        }
        else
        {
            pdbFound = false;
            Logger.LogDebug("Found mdb and pdb debug symbols. Selected mdb (newer).");
        }
    }

    void FindPdb()
    {
        // because UWP use a wacky convention for symbols
        pdbPath = Path.ChangeExtension(AssemblyFilePath, "compile.pdb");
        if (File.Exists(pdbPath))
        {
            pdbFound = true;
            Logger.LogDebug($"Found debug symbols at '{pdbPath}'.");
            return;
        }
        pdbPath = Path.ChangeExtension(AssemblyFilePath, "pdb");
        if (File.Exists(pdbPath))
        {
            pdbFound = true;
            Logger.LogDebug($"Found debug symbols at '{pdbPath}'.");
        }
    }

    void FindMdb()
    {
        mdbPath = AssemblyFilePath + ".mdb";
        if (File.Exists(mdbPath))
        {
            mdbFound = true;
            Logger.LogDebug($"Found debug symbols at '{mdbPath}'.");
        }
    }
}