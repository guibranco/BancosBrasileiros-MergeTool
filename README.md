# Bancos Brasileiros - Merge Tool

<p align="center">
  <img src="https://raw.githubusercontent.com/guibranco/BancosBrasileiros-MergeTool/main/logo.png" alt="Bancos Brasileiros logo" width="300"/>
</p>

<p align="center">
  🇧🇷 🏦 📋 A tool for generating and updating data in the <a href="https://github.com/guibranco/BancosBrasileiros/">Bancos Brasileiros</a> repository
</p>

<p align="center">
  <a href="https://ci.appveyor.com/project/guibranco/bancosbrasileiros-MergeTool"><img src="https://ci.appveyor.com/api/projects/status/f9sx7ux82epp8bd6?svg=true" alt="Build status"></a>
  <a href="https://github.com/guibranco/BancosBrasileiros-MergeTool/commits/main"><img src="https://img.shields.io/github/last-commit/guibranco/BancosBrasileiros-MergeTool" alt="GitHub last commit"></a>
  <a href="https://github.com/guibranco/BancosBrasileiros-MergeTool/blob/main/LICENSE"><img src="https://img.shields.io/github/license/guibranco/BancosBrasileiros-MergeTool" alt="GitHub license"></a>
  <a href="https://wakatime.com/badge/github/guibranco/BancosBrasileiros-MergeTool"><img src="https://wakatime.com/badge/github/guibranco/BancosBrasileiros-MergeTool.svg" alt="time tracker"></a>
</p>

---

## 📋 About

The BancosBrasileiros-MergeTool is a utility designed to gather, process, and consolidate Brazilian banking institution data from multiple official sources. This tool ensures that the [Bancos Brasileiros](https://github.com/guibranco/BancosBrasileiros/) repository contains the most up-to-date and accurate information about financial institutions in Brazil.

## 🔄 Data Sources

The tool currently collects and merges data from the following sources:

| Source | Description | Data Type |
|--------|-------------|-----------|
| [Bancos.json](https://github.com/guibranco/BancosBrasileiros/blob/main/data/bancos.json) | Source of truth - base JSON with trusted data | JSON |
| [STR](https://www.bcb.gov.br/content/estabilidadefinanceira/str1/ParticipantesSTR.csv) | Sistema de Transferência de Reservas | CSV |
| [SPI/PIX](https://github.com/guibranco/BancosBrasileiros-MergeTool/blob/main/BancosBrasileiros.MergeTool/Helpers/Constants.cs#L45) | Sistema de Pagamentos Instantâneos | CSV |
| [SLC](https://www2.nuclea.com.br/Monitoramento/Participantes%20Homologados.pdf) | Serviço de Liquidação Cartões | PDF |
| [SILOC](https://www2.nuclea.com.br/Monitoramento/SILOC.pdf) | Sistema de Liquidação Diferida das Transferências Interbancárias de Ordens de Crédito | PDF |
| [SITRAF](https://www2.nuclea.com.br/Monitoramento/Rela%C3%A7%C3%A3o%20de%20Clientes%20SITRAF.pdf) | Sistema de Transferência de Fundos | PDF |
| [CTC](https://www2.nuclea.com.br/SAP/Rela%C3%A7%C3%A3o%20de%20Clientes%20CTC.pdf) | Central de Transferência de Crédito | PDF |
| [PCPS](https://www2.nuclea.com.br/SAP/Rela%C3%A7%C3%A3o%20de%20Participantes%20PCPS.pdf) | Plataforma Centralizada de Portabilidade de Salário | PDF |
| [PCR](https://www2.nuclea.com.br/SAP/Rela%C3%A7%C3%A3o%20de%20Clientes%20PCR.pdf) | Plataforma Centralizada de Recebíveis | PDF |
| [Cheque Legal](https://www2.nuclea.com.br/SAP/Rela%C3%A7%C3%A3o%20de%20Participantes%20CQL.pdf) | Cheque Legal | PDF |
| [Detecta Flow](https://www2.nuclea.com.br/SAP/Rela%C3%A7%C3%A3o%20de%20Participantes%20-%20Detecta%20Flow.pdf) | Detecta Flow | PDF |

## 💻 Technology

- **Language**: C# (.NET 9.0)
- **Features**: PDF data extraction, data merging, output in multiple formats

## 🚀 Getting Started

### Prerequisites

- .NET 9.0 SDK or later
- Your preferred IDE (Visual Studio, VS Code, Rider, etc.)

### Running Locally

1. Clone the repository:
   ```bash
   git clone https://github.com/guibranco/BancosBrasileiros-MergeTool.git
   ```

2. Navigate to the project directory:
   ```bash
   cd BancosBrasileiros-MergeTool
   ```

3. Build and run the project:
   ```bash
   dotnet build
   dotnet run
   ```

Output files will be generated in the `result` directory.

## 🤝 Contributing

We welcome contributions to improve the MergeTool! Here's how to add a new data source:

### Step-by-Step Guide

1. **Fork and Clone** the repository:
   ```bash
   git clone https://github.com/YourUsername/BancosBrasileiros-MergeTool.git
   ```

2. **Open the project** in your preferred IDE.

3. **Add Required Information**:
   - Add URLs to `Constants.cs`
   - Add a new enum item in `Source.cs` (use system acronyms when possible)

4. **Implement Data Reader**:
   - Create a new method in `Reader.cs` named `Load[NewSystemAcronym]`
   - For PDF sources, follow the existing patterns for extraction
   - For other formats, implement appropriate extraction logic
   - If using RegExp, add patterns to `Patterns.cs`

5. **Update Data Model**:
   - Add new fields to the `Bank.cs` file

6. **Implement Data Merging**:
   - Create a method in `Seeder.cs` named `Merge[NewSystemAcronym]`
   - Filter data by ISPB and Document to find existing entries
   - Only add new banks if they have COMPE, ISPB, Document, and Name (minimum required fields)

7. **Update Program Flow**:
   - Call your new methods in the `AcquireData` method in `Program.cs`

8. **Update Output Writers**:
   - Edit the following methods in `Writer.cs` to include your new fields:
     - `SaveCsv`
     - `SaveMarkdown`
     - `SaveSql`

9. **Test Your Changes**:
   - Run the application locally to verify it works correctly
   - Check the generated files in the `result` directory

10. **Submit a Pull Request**:
    - Commit your changes
    - Push to your fork
    - Create a PR with a detailed description of your changes

### Important Notes

- **Testing**: You can run the application locally as many times as needed without affecting the repository
- **New Banks**: If adding new bank entries, please mention this in your PR comment
- **Mandatory Fields**: Ensure all new bank entries have COMPE, ISPB, Document, and Name at minimum

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgements

- Thanks to all contributors who have helped improve this tool
- Thanks to the Brazilian financial institutions for providing the data sources
