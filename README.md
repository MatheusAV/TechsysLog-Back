# TechsysLog – Backend API

Backend do sistema **TechsysLog**, responsável pelo controle de **usuários, pedidos, entregas e notificações em tempo real**, desenvolvido com **ASP.NET Core**, **MongoDB**, **JWT** e **SignalR**.

---

## 📌 Visão Geral

Esta API foi desenvolvida para atender ao cenário de uma empresa de logística que necessita:

- Cadastro de usuários
- Cadastro de pedidos
- Registro de entregas
- Notificações em tempo real para os usuários
- Histórico de notificações já visualizadas
- Segurança via autenticação JWT

A solução foi construída priorizando **organização, escalabilidade, boas práticas, separação de responsabilidades e segurança**.

---

## 🏗️ Arquitetura

A aplicação segue uma arquitetura **modular e desacoplada**, inspirada em **Clean Architecture** e **DDD (Domain-Driven Design)**.


### Principais decisões arquiteturais

- **Controllers finos**, delegando regras para a camada de Application
- **Serviços de domínio isolados**
- **Infraestrutura desacoplada via interfaces**
- **MongoDB** como banco NoSQL para flexibilidade de esquema
- **SignalR** para comunicação em tempo real
- **JWT** para autenticação e proteção das rotas

---

## 🔐 Segurança

- Autenticação baseada em **JWT (JSON Web Token)**
- Rotas protegidas via `[Authorize]`
- Tokens configuráveis via `appsettings.json`
- Expiração configurável do token

---

## 🗄️ Banco de Dados

- **MongoDB**
- Collections separadas por contexto:
  - Users
  - Orders
  - Deliveries
  - Notifications

- Índices únicos aplicados onde necessário (ex: e-mail do usuário)
- Documentos modelados para leitura eficiente

---

## 🔔 Notificações em Tempo Real

Utilizado **SignalR** para:

- Atualizar painel de pedidos em tempo real
- Notificar usuários quando:
  - Um pedido é criado
  - Uma entrega é registrada
- Manter histórico de notificações já visualizadas

---

## 🌐 Integrações Externas

- Integração com **API de CEP** para preenchimento automático de endereço
- Isolada em serviço próprio para facilitar manutenção e testes

---

## 🚀 Tecnologias Utilizadas

- **ASP.NET Core**
- **C#**
- **MongoDB**
- **SignalR**
- **JWT**
- **Swagger**
- **Docker (opcional)**
- **xUnit / Moq** (testes)

---

## ⚙️ Configuração do Projeto

### Pré-requisitos

- .NET SDK
- MongoDB
- Docker (opcional)

---

### Configuração do `appsettings.json`

```json
{
  "MongoDb": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "techsyslog"
  },
  "Jwt": {
    "Issuer": "techsyslog",
    "Audience": "techsyslog",
    "SecretKey": "CHANGE_THIS_TO_A_LONG_RANDOM_SECRET_KEY_32+",
    "ExpirationMinutes": 60
  }
}
