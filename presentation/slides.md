---
theme: seriph
background: https://images.unsplash.com/photo-1451187580459-43490279c0fa?q=80&w=2072&auto=format&fit=crop
class: text-center
highlighter: shiki
lineNumbers: true
info: |
  ## Smart Invoice Automation System
  Automating invoice processing witdh Azure AI
drawings:
  persist: false
transition: fade-out
title: Smart Invoice Automation System
mdc: true
fonts:
  sans: 'Inter'
  serif: 'Robot Slab'
  mono: 'Fira Code'
---

# Smart Invoice Automation System

## Processing Invoices in Seconds with Azure AI

<div class="absolute bottom-10 left-10 right-10">
  <div class="text-sm opacity-75">
    Powered by Azure AI Services
  </div>
</div>


---
transition: fade
layout: center
class: text-center
---

# The Problem

<div class="pt-4 text-left">

## Manual invoice processing takes 3-5 minutes per invoice

<v-clicks>

- 📄 Read vendor name and invoice number
- 📅 Extract date and total amount
- 🏷️ Manually categorize (IT, Office, Marketing, etc.)
- ⌨️ Enter data into accounting system

</v-clicks>

</div>

<v-click>

<div class="pt-6 text-xl text-red-400">
Hours of repetitive work × hundreds of invoices per month
</div>

</v-click>


---
transition: fade
layout: center
class: text-center
---

# The Solution

<div class="text-left">

## Automated with Azure AI

<v-clicks>

- 📤 **Upload** - PDF/image
- 🤖 **Extract** - Form Recognizer
- 🧠 **Classify** - OpenAI
- ⚡ **Results** - Seconds

</v-clicks>

</div>

<v-click>

<div class="pt-10 pl-8">

```mermaid
graph LR
    A[Upload] --> B[Extract]
    B --> C[Classify]
    C --> D[Store]
    style A fill:#0078d4,stroke:#0078d4,stroke-width:3px,color:#fff
    style B fill:#00bcf2,stroke:#00bcf2,stroke-width:3px,color:#000
    style C fill:#00d4aa,stroke:#00d4aa,stroke-width:3px,color:#000
    style D fill:#e81123,stroke:#e81123,stroke-width:3px,color:#fff
```

</div>

</v-click>



---
transition: fade
layout: center
class: text-center
---

# Architecture Overview

<div>

```mermaid
graph TB
    A[Next.js Frontend] --> B[Azure Functions]
    B --> C[Form Recognizer]
    B --> D[Azure OpenAI]
    B --> E[Blob Storage]
    B --> F[Cosmos DB]
    style A fill:#0078d4,stroke:#0078d4,stroke-width:3px,color:#fff
    style B fill:#0078d4,stroke:#0078d4,stroke-width:3px,color:#fff
    style C fill:#00bcf2,stroke:#00bcf2,stroke-width:3px,color:#000
    style D fill:#00d4aa,stroke:#00d4aa,stroke-width:3px,color:#000
    style E fill:#e81123,stroke:#e81123,stroke-width:3px,color:#fff
    style F fill:#e81123,stroke:#e81123,stroke-width:3px,color:#fff
```

</div>




---
transition: fade
layout: center
class: text-center
---

# The Processing Pipeline

<div class="pb-6">

```mermaid
graph LR
    A[📤 Upload] --> B[📁 Blob Storage]
    B --> C[🤖 Form Recognizer]
    C --> D[🧠 OpenAI]
    D --> E[💾 Cosmos DB]
    E --> F[✨ Display]
    style A fill:#0078d4,stroke:#0078d4,stroke-width:3px,color:#fff
    style B fill:#e81123,stroke:#e81123,stroke-width:3px,color:#fff
    style C fill:#00bcf2,stroke:#00bcf2,stroke-width:3px,color:#000
    style D fill:#00d4aa,stroke:#00d4aa,stroke-width:3px,color:#000
    style E fill:#e81123,stroke:#e81123,stroke-width:3px,color:#fff
    style F fill:#0078d4,stroke:#0078d4,stroke-width:3px,color:#fff
```

</div>

<div class="grid grid-cols-3 gap-8 px-12 text-sm text-left">

<div v-click>

**Extract Data**
- Form Recognizer analyzes invoice
- Extracts structured data

</div>

<div v-click>

**Classify**
- OpenAI categorizes invoice
- Returns confidence + reasoning

</div>

<div v-click>

**Store & Display**
- Saved to Cosmos DB
- Displayed to user in real-time

</div>

</div>


--- 
transition: fade
layout: center
class: text-center
---

# Live Demo!



---
transition: fade
layout: center
class: text-center
---

# Key Takeaways

<div class="pt-4 text-left">

<v-clicks>

## 🎯 Azure AI solves real problems
Form Recognizer + OpenAI = powerful automation

## ⚡ Serverless is cost-effective
Perfect for MVPs and production

## 🚀 Modern tooling accelerates dev
Next.js + .NET + TypeScript

## 🔧 Production-ready
Scalable and maintainable

</v-clicks>

</div>

---
transition: fade
layout: center
class: text-center
---

# Production Considerations

<div class="grid grid-cols-2 gap-6 pt-3 text-left">

<div>

## Security

<v-clicks>

- 🔐 Authentication
- 🚦 Rate limiting
- 🛡️ Input validation
- 🔄 Retry logic
- 📝 Logging

</v-clicks>

</div>

<div>

## Features

<v-clicks>

- 📊 App Insights
- 📤 Bulk uploads
- 🔗 Integrations
- 📥 Export (CSV/Excel)
- 🌍 Multi-region

</v-clicks>

</div>

</div>

<v-click>

<div class="pt-6 text-center text-lg opacity-75">
Production-ready with these enhancements
</div>

</v-click>



---
layout: center
class: text-center
---

# Thank You!


<div class="pt-12 opacity-75">

**Ehsan Tatasadi** | Cloud & AI Automation

<div class="pt-4">
🔗 github.com/tatasadi
</div>

<div class="pt-2">
💼 linkedin.com/in/ehsan-tatasadi
</div>

</div>

<div class="pt-12 text-sm opacity-50">
If you found this useful, please subscribe and drop a comment!
</div>


