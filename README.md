# 🔐 Console Password Manager

<div align="center">

![C#](https://img.shields.io/badge/C%23-12.0-239120?style=for-the-badge&logo=csharp)
![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet)

**Secure password manager with AES encryption**

</div>

---

## 📖 About

Console-based password manager with master password protection and AES encryption. Store, generate, and manage your passwords securely.

---

## ✨ Features

| Feature | Description |
|---------|-------------|
| 🔐 Master password | SHA256 hashed access control |
| 🔒 AES encryption | All passwords stored encrypted |
| ➕ Add passwords | Service, username, password, URL, notes |
| 🎲 Password generator | Generate strong random passwords |
| 🔍 Search | Search by service name |
| ✏️ Edit | Update existing entries |
| 🗑️ Delete | Remove unwanted entries |
| 📊 Statistics | Password count, distribution |
| 💾 Export CSV | Export all passwords to file |

---

## 🚀 Quick Start

```bash
# Create project
mkdir PasswordManager && cd PasswordManager
dotnet new console

# Install package
dotnet add package System.Security.Cryptography.ProtectedData

# Run
dotnet run
```

---

## 🎮 Usage

- First run - Create master password  
- Login - Enter master password  

Main menu:

1 - List all passwords  
2 - Add new password  
3 - Search password  
4 - Edit password  
5 - Delete password  
6 - Statistics  
7 - Export to CSV  
8 - Exit  

---

## 📁 Files Created

| File                    | Purpose                     |
|-------------------------|-----------------------------|
| `master.hash`           | Master password hash        |
| `passwords.dat`         | Encrypted password database |
| `key.dat`               | Encrypted AES key           |
| `iv.dat`                | AES initialization vector   |
| `passwords_export_*.csv`| Exported passwords          |

---

## 🔐 Security

- Master password never stored in plain text  
- AES-256 encryption for all data  
- DPAPI protection for encryption keys  
- Password masked with asterisks on input  
