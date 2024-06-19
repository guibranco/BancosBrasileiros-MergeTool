# Bancos Brasileiros - Merge Tool

🇧🇷 🏦 📋 The **Merge Tool** generates and updates data in the [Bancos Brasileiros](https://github.com/guibranco/BancosBrasileiros/) repository.

[![Build status](https://ci.appveyor.com/api/projects/status/f9sx7ux82epp8bd6?svg=true)](https://ci.appveyor.com/project/guibranco/bancosbrasileiros-MergeTool)
[![GitHub last commit](https://img.shields.io/github/last-commit/guibranco/BancosBrasileiros-MergeTool)](https://wakatime.com/badge/github/guibranco/BancosBrasileiros-MergeTool)
[![GitHub license](https://img.shields.io/github/license/guibranco/BancosBrasileiros-MergeTool)](https://wakatime.com/badge/github/guibranco/BancosBrasileiros-MergeTool)
[![time tracker](https://wakatime.com/badge/github/guibranco/BancosBrasileiros-MergeTool.svg)](https://wakatime.com/badge/github/guibranco/BancosBrasileiros-MergeTool)

![Bancos Brasileiros logo](https://raw.githubusercontent.com/guibranco/BancosBrasileiros-MergeTool/main/logo.png)

---

## Contributing

Here is a step-by-step on how to add a new source of data to the merge tool:

-  Check out [MergeTool](https://github.com/guibranco/BancosBrasileiros-MergeTool).
-  Open VS, VS Code, Rider, or your favorite IDE / Code Editor for .NET projects.
-  Load the project (currently, it is in **C# .NET 8.0**).
-  Add the required URLs to the `Constants.cs` file.
-  Add a new enum item in the `Source.cs` file. Please use the source system acronyms whenever possible.
-  Add a new method in the `Reader.cs` called **Load\[NewSystemAcronym]**. This should do all the heavy job of grabbing the information from the remote source.
-  If the new source provides a PDF file, follow the other method patterns to extract the PDF information from the file.
-  Implements the data extraction the way you prefer if it is not a PDF file.
-  If you need to use RegExp to extract data, add it to the `Patterns.cs` file.
-  Add the new field(s) to the `Bank.cs` file.
-  In the `Seeder.cs` file, implement the method **Merge\[NewSystemAcronym]** to merge the new data with the existing ones. I prefer to filter the data by *ISPB* and then *Document* to check for existing data. Rely on the existing list. **DO NOT ADD** new bank to the list if it is not present with **COMPE, ISPB, Document, and Name at least**. These are mandatory fields, if you have all this information, and you did not find the bank on the existing list, feel free to add it to the list. (Let me know this in the PR comment).
- Call the new methods (**Load\[NewSystemAcronym]r** and **Merge\[NewSystemAcronym]**) in the `AcquireData` method inside the `Program.cs` file.
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

Currently, this tool uses some sources to generate data. The complete list of sources can be found at [Constants.cs](https://github.com/guibranco/BancosBrasileiros-MergeTool/blob/main/BancosBrasileiros.MergeTool/Helpers/Constants.cs)

- [Bancos.json](https://github.com/guibranco/BancosBrasileiros/blob/main/data/bancos.json) - Source of truth - the base JSON file with trusted data. Any manual change should be done in this file.
- [STR](https://www.bcb.gov.br/content/estabilidadefinanceira/str1/ParticipantesSTR.csv) - The STR (Sistema de Transferência de Reservas).
- [SPI](https://github.com/guibranco/BancosBrasileiros-MergeTool/blob/main/BancosBrasileiros.MergeTool/Helpers/Constants.cs#L45) - The SPI/PIX (Sistema de Pagamentos Instantâneos) PSP (Participantes do Sistema de Pagamentos), always with the current date.
- [SLC](https://www2.nuclea.com.br/Monitoramento/Participantes_Homologados.pdf) - The SLC (Serviço de Liquidação Cartões).
- [SILOC](https://www2.nuclea.com.br/Monitoramento/SILOC.pdf) - The SILOC (Sistema de Liquidação Diferida das Transferências Interbancárias de Ordens de Crédito).
- [SITRAF](https://www2.nuclea.com.br/Monitoramento/Rela%C3%A7%C3%A3o%20de%20Clientes%20SITRAF.pdf) - The SITRAF (Sistema de Transferência de Fundos).
- [CTC](https://www2.nuclea.com.br/SAP/Rela%C3%A7%C3%A3o%20de%20Clientes%20CTC.pdf) - The CTC (Central de Transferência de Crédito).
- [PCPS](https://www2.nuclea.com.br/SAP/Rela%C3%A7%C3%A3o%20de%20Participantes%20PCPS.pdf) - The PCPS (Plataforma Centralizada de Portabilidade de Salário).
- [PCR](https://www2.nuclea.com.br/SAP/Rela%C3%A7%C3%A3o%20de%20Clientes%20PCR.pdf) - The PCR (Plataforma Centralizada de Recebíveis).
- [Cheque Legal](https://www2.nuclea.com.br/SAP/Rela%C3%A7%C3%A3o%20de%20Participantes%20CQL.pdf) - The Cheque Legal.
- [Detecta Flow](https://www2.nuclea.com.br/SAP/Rela%C3%A7%C3%A3o%20de%20Participantes%20-%20Detecta%20Flow.pdf) - The Detecta Flow.
