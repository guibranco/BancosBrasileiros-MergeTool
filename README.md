# Bancos Brasileiros - Merge Tool

[![Build status](https://ci.appveyor.com/api/projects/status/f9sx7ux82epp8bd6?svg=true)](https://ci.appveyor.com/project/guibranco/bancosbrasileiros-MergeTool)
[![GitHub last commit](https://img.shields.io/github/last-commit/guibranco/BancosBrasileiros-MergeTool)](https://wakatime.com/badge/github/guibranco/BancosBrasileiros-MergeTool)
[![GitHub license](https://img.shields.io/github/license/guibranco/BancosBrasileiros-MergeTool)](https://wakatime.com/badge/github/guibranco/BancosBrasileiros-MergeTool)
[![time tracker](https://wakatime.com/badge/github/guibranco/BancosBrasileiros-MergeTool.svg)](https://wakatime.com/badge/github/guibranco/BancosBrasileiros-MergeTool)

![Bancos Brasileiros logo](https://raw.githubusercontent.com/guibranco/BancosBrasileiros-MergeTool/main/logo.png)

The **Merge Tool** is used to generate and keep updated data of [Bancos Brasileiros](https://github.com/guibranco/BancosBrasileiros/) repository.

---

## Contributing

Here is a step-by-step on how to add a new source of data to the merge tool:

-  Check out [MergeTool](https://github.com/guibranco/BancosBrasileiros-MergeTool).
-  Open VS, VS Code, Rider, or your favourite IDE / Code Editor for .NET projects.
-  Load the project (currently it is in **C# .NET 7.0**).
-  Add the required URLs to the `Constants.cs` file.
-  Add a new enum item in the `Source.cs` file, use the source system acronyms when possible.
-  Add a new method in the `Reader.cs` called **Load\[NewSystemAcronym]**, this should do all the heavy job of grabbing the information from the remote source.
-  Follow the other methods patterns to extract the PDF information from the file if the new source provides a PDF file.
-  Implements the data extraction the way you prefer if it is not a PDF file.
-  Add the RegExp to the `Patterns.cs` file if you need to use RegExp to extract data.
-  Add the new field(s) to `Bank.cs` file.
-  In the `Seeder.cs` file, implement the method **Merge\[NewSystemAcronym]** to merge the new data with the existing ones. Prefer to filter the data by *ISPB*, then *Document* to check for existing data. Rely on the existing list, **DO NOT ADD** new bank to the list if it is not present with **COMPE, ISPB, Document, and Name at least**. These are mandatory fields, if you have all this information, and you did not find the bank on the existing list, feel free to add it to the list. (Let me know this in the PR comment).
-  Call the new methods (**Load\[NewSystemAcronym]r** and **Merge\[NewSystemAcronym]**) in `AcquireData` method inside `Program.cs` file.
-  On the `Writer.cs` file, edit the following methods, mapping the new field(s):
   -  `SaveCsv`
   -  `SaveMarkdown`
   -  `SaveSql`
 -  Test it 🧪 
 -  Commit and submit a PR 🎉

### Testing

-  You can run the application locally without submitting any changes to this repository.
-  Run how many times you need, it will only generate some files in the output directory inside **result** directory.
  
