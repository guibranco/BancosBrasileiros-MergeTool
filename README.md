# Bancos Brasileiros - Merge Tool

🇧🇷 🏦 📋 The **Merge Tool** is used to generate and keep data in the [Bancos Brasileiros](https://github.com/guibranco/BancosBrasileiros/) repository updated.

[![Build status](https://ci.appveyor.com/api/projects/status/f9sx7ux82epp8bd6?svg=true)](https://ci.appveyor.com/project/guibranco/bancosbrasileiros-MergeTool)
[![GitHub last commit](https://img.shields.io/github/last-commit/guibranco/BancosBrasileiros-MergeTool)](https://wakatime.com/badge/github/guibranco/BancosBrasileiros-MergeTool)
[![GitHub license](https://img.shields.io/github/license/guibranco/BancosBrasileiros-MergeTool)](https://wakatime.com/badge/github/guibranco/BancosBrasileiros-MergeTool)
[![time tracker](https://wakatime.com/badge/github/guibranco/BancosBrasileiros-MergeTool.svg)](https://wakatime.com/badge/github/guibranco/BancosBrasileiros-MergeTool)

![Bancos Brasileiros logo](https://raw.githubusercontent.com/guibranco/BancosBrasileiros-MergeTool/main/logo.png)

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

### Sources

Currently this tool use some sources for the data generated. The complete list of sources can be found at [Constants.cs](https://github.com/guibranco/BancosBrasileiros-MergeTool/blob/main/BancosBrasileiros.MergeTool/Helpers/Constants.cs)

- [Bancos.json](https://github.com/guibranco/BancosBrasileiros/blob/main/data/bancos.json) - Source of truth - the base JSON file with trusted data. Any manual change should be done in this file.
- [STR](https://www.bcb.gov.br/content/estabilidadefinanceira/str1/ParticipantesSTR.csv) - The STR (Sistema de Transferência de Reservas) file.
- [SPI](https://www.bcb.gov.br/content/estabilidadefinanceira/spi/participantes-spi-20240101.csv) - The SPI/PIX participants (SPI PSPs), always with the current date.
- [SLC](https://www2.nuclea.com.br/Monitoramento/Participantes_Homologados.pdf) - The SLC (Serviço de Liquidação Cartões)
- [SILOC](https://www2.nuclea.com.br/Monitoramento/SILOC.pdf) - The SILOC
- [SITRAF](https://www2.nuclea.com.br/Monitoramento/Rela%C3%A7%C3%A3o%20de%20Clientes%20SITRAF.pdf) - The SITRAF
- [CTC](https://www2.nuclea.com.br/SAP/CTC.pdf) - The CTC
- [PCPS](https://www2.nuclea.com.br/SAP/Rela%C3%A7%C3%A3o%20de%20Participantes%20PCPS.pdf) - The PCPS
- [Cheque Legal](https://www2.nuclea.com.br/SAP/Rela%C3%A7%C3%A3o%20de%20Participantes%20CQL.pdf) - The Cheque Legal
- [DetectaFlow](https://www2.nuclea.com.br/SAP/Rela%C3%A7%C3%A3o%20de%20Participantes%20-%20Detecta%20Flow.pdf) - The DetectaFlow
