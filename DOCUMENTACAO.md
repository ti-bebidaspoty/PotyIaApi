# PotyIaApi — Documentação do Projeto

Compilado técnico da API, descrevendo cada componente, suas responsabilidades e como eles se integram entre si.

---

## 1. Visão geral

**PotyIaApi** é uma Web API em **ASP.NET Core (.NET 10)** que expõe endpoints REST para:

- **Cadastro de usuários** a partir de dados vindos do sistema **Senior (VETORH)**.
- **Autenticação** por CPF + senha, emitindo **JWT (Access Token)** e **Refresh Token**.
- **Renovação de tokens** com rotação e proteção contra reutilização.

A documentação interativa dos endpoints é servida via **Swagger** (a raiz `/` redireciona para `/swagger`).

### Stack e pacotes

| Item | Valor |
|------|-------|
| Target Framework | `net10.0` |
| Nullable / ImplicitUsings | Habilitados |
| Autenticação | `Microsoft.AspNetCore.Authentication.JwtBearer` 10.0.10 |
| Acesso a dados | `Microsoft.Data.SqlClient` 7.0.2 / `System.Data.SqlClient` 4.8.6 (ADO.NET puro) |
| Serialização | `Newtonsoft.Json` 13.0.3 + `System.Text.Json` |
| Documentação | `Swashbuckle.AspNetCore` 10.2.3 (Swagger) |

---

## 2. Arquitetura em camadas

O projeto segue uma separação clássica em camadas, com injeção de dependência via interfaces:

```
HTTP Request
	│
	▼
┌─────────────────┐   Controllers (API / rotas / status HTTP)
│   Controllers   │   AutenticacaoController, UsuarioController, BasicaController
└────────┬────────┘
		 │  chama
		 ▼
┌─────────────────┐   Services (regras de negócio / orquestração)
│    Services     │   AutenticacaoService, RefreshTokenService, UsuarioService
└────────┬────────┘
		 │  usa (via interface)
		 ▼
┌─────────────────┐   Repositories (acesso a dados — ADO.NET)
│  Repositories   │   AutenticacaoRepositorio, UsuarioRepositorio,
│                 │   RefreshTokenRepositorio, BasicoRepositorio
└────────┬────────┘
		 │  consulta
		 ▼
┌─────────────────┐   Banco SQL Server
│   Banco (SQL)   │   InternosPoty (PotyIA.*) + Senior (VETORH_PROD)
└─────────────────┘

Transversais: Models (DTOs/entidades), Helpers (criptografia), Interfaces (contratos)
```

**Fluxo de dependência:** Controller → Service → Interface do Repositório → Repositório concreto → Banco.
As camadas dependem de **abstrações (interfaces)**, e as implementações concretas são resolvidas pelo container de DI registrado no `Program.cs`.

---

## 3. Estrutura de pastas

```
PotyIaApi/
├── Program.cs                  # Bootstrap, DI, JWT, CORS, Swagger, pipeline
├── PotyIaApi.csproj            # Target net10.0 e dependências
├── appsettings.json            # Connection strings + configurações JWT
├── Controllers/
│   ├── BasicaController.cs      # Controller base (retornoApi padronizado)
│   ├── AutenticacaoController.cs
│   └── UsuarioController.cs
├── Services/
│   ├── AutenticacaoService.cs
│   ├── RefreshTokenService.cs
│   └── UsuarioService.cs
├── Repositories/
│   ├── BasicoRepositorio.cs     # Conexão/abertura/fechamento SQL
│   ├── AutenticacaoRepositorio.cs
│   ├── UsuarioRepositorio.cs
│   └── RefreshTokenRepositorio.cs
├── Interfaces/                  # Contratos de cada componente injetável
├── Models/                     # DTOs e entidades
├── Helpers/Helper.cs           # Hash SHA-256
└── Database/RefreshTokens.sql  # DDL da tabela de refresh tokens
```

---

## 4. Configuração e inicialização (`Program.cs`)

Ordem de configuração no bootstrap:

1. **Validação do JWT** — lê `Jwt:Key`, `Jwt:Issuer`, `Jwt:Audience` e lança `InvalidOperationException` se algum estiver ausente (falha rápida na inicialização).
2. **Controllers** — `AddControllers()` com `PropertyNamingPolicy = null` (mantém os nomes das propriedades como no C#, sem camelCase).
3. **Injeção de dependência** (todos como `Scoped` — uma instância por requisição):
   - `IHelper → Helper`
   - `IBasicoRepositorio → BasicoRepositorio`
   - `IAutenticacaoRepositorio → AutenticacaoRepositorio`
   - `IUsuarioRepositorio → UsuarioRepositorio`
   - `IRefreshTokenRepositorio → RefreshTokenRepositorio`
   - `AutenticacaoService`, `RefreshTokenService`, `UsuarioService` (classes concretas)
4. **CORS** — política `CorsLiberado` liberando qualquer origem, método e header.
5. **Swagger** — `AddEndpointsApiExplorer()` + `AddSwaggerGen()`.
6. **Autenticação JWT** — `TokenValidationParameters` valida emissor, audiência, tempo de vida e assinatura (`ClockSkew = Zero`). O evento `OnChallenge` é sobrescrito para retornar **401 em JSON** padronizado (`Status` + `Mensagem`), em vez do challenge padrão.
7. **Pipeline** — Swagger → redirect `/` → `/swagger` → CORS → Authentication → Authorization → `MapControllers()`.

> Observação: `UseHttpsRedirection()` está **comentado** (ambiente permite HTTP).

---

## 5. Componentes detalhados

### 5.1 Controllers

#### `BasicaController` (base)
Herda de `ControllerBase` e expõe o método `retornoApi(object? retorno, int codigoErro, string mensagemErro)`:
- Se `retorno == null`: monta um `ResponseModel { Status, Resposta }` e retorna `StatusCode(codigoErro, ...)`.
- Caso contrário: retorna `200` com o objeto de dados.

Todos os demais controllers herdam dele para padronizar as respostas.

#### `AutenticacaoController` — rota base `api/Autenticacao`
| Método | Rota | Descrição |
|--------|------|-----------|
| `POST` | `api/Autenticacao` | Recebe `AutenticacaoModel` (CPF/Senha). Chama `AutenticacaoService.RealizarAutenticacao`. Se válido, gera tokens via `RefreshTokenService.GerarTokens` e retorna `AccessToken`, `RefreshToken`, `RefreshTokenExpiraEm` e dados do usuário. Credenciais inválidas → 400. |
| `POST` | `api/Autenticacao/refresh-token` | Recebe `RefreshTokenRequestModel`. Chama `RefreshTokenService.RenovarTokens`. Retorna sempre **401 genérico** em qualquer falha (não revela se o token expirou, foi revogado ou não existe). |

#### `UsuarioController` — rota base `api/Usuario`
| Método | Rota | Descrição |
|--------|------|-----------|
| `POST` | `api/Usuario/{cpf}` | Chama `UsuarioService.CadastrarUsuario(cpf)`. Sucesso → 201; erro → 500 com mensagem. |

---

### 5.2 Services (regras de negócio)

#### `AutenticacaoService`
Fina camada sobre `IAutenticacaoRepositorio`. Delega `RealizarAutenticacao` ao repositório e propaga exceções.

#### `UsuarioService`
Orquestra o cadastro de usuário:
1. `VerificarUsuarioJaCadastrado(cpf)` — se já existe em `PotyIA.Usuarios`, lança exceção.
2. `VerificarUsuarioSenior(cpf)` — busca os dados no banco **Senior (VETORH)**; se não encontrado, lança exceção.
3. Persiste o novo usuário via `IUsuarioRepositorio.CadastrarUsuario`.

> A senha inicial do usuário é a **data de nascimento** vinda do Senior, já criptografada (SHA-256) no repositório.

#### `RefreshTokenService` (núcleo de segurança)
Responsável por gerar e renovar tokens. Principais métodos:

- **`GerarAccessToken(usuario)`** — cria um JWT HMAC-SHA256 com claims `NameIdentifier` (UsuarioID) e `Name`, expiração de `Jwt:AccessTokenExpirationMinutes` (default 120 min).
- **`GerarTokens(usuario)`** (login) — gera Access Token + cria e persiste um Refresh Token, retornando `TokensRespostaModel`.
- **`RenovarTokens(refreshTokenPuro)`** (refresh com rotação):
  1. Calcula o hash do token recebido (`IHelper.Criptografar`) e busca por hash.
  2. Token inexistente → falha genérica.
  3. **Token já revogado sendo reapresentado** → revoga **toda a cadeia do usuário** (defesa contra reutilização/roubo) e falha.
  4. Token expirado → falha.
  5. Caso válido: **rotaciona** — cria novo refresh token, insere no banco e revoga o antigo apontando para o novo (`SubstituidoPorTokenHash`). Emite novo Access Token.
- **`CriarRefreshToken(usuarioID)`** — gera token aleatório (64 bytes via `RandomNumberGenerator`, Base64), armazena apenas o **hash**, define `CriadoEm`/`ExpiraEm` (default 7 dias).
- **`ResultadoRefreshToken`** — DTO com `Sucesso` e `Tokens` (erros sempre genéricos).

---

### 5.3 Repositories (acesso a dados — ADO.NET)

#### `BasicoRepositorio` (base)
Centraliza o gerenciamento de conexões SQL:
- `BuscarConexao()` → connection string **`InternosPoty`**.
- `BuscarConexaoSenior()` → connection string **`Senior`**.
- `AbrirConexao(con)` → abre com **retry (até 3 tentativas)**; em `SqlException` limpa o pool (`ClearPool`) e aguarda `200ms * tentativa` (mitiga conexões “mortas” no pool após ociosidade/firewall).
- `FecharConexao(con)` → `con.Close()`.

Todos os repositórios concretos herdam dele.

#### `AutenticacaoRepositorio`
`RealizarAutenticacao` consulta `PotyIA.Usuarios` por `CPF` + `Senha` (senha comparada já criptografada via `IHelper`) e `Ativo = 1`, retornando `UsuarioAutenticadoModel` (UsuarioID, Nome).

#### `UsuarioRepositorio`
- `CadastrarUsuario` — `INSERT` em `PotyIA.Usuarios` (UsuarioID via `NEWID()`, Ativo = 1).
- `VerificarUsuarioSenior` — consulta `VETORH_PROD.dbo.R034FUN` (Senior) por CPF, funcionário **ativo** (`SITAFA <> 7` e `DATAFA = '19001231'`), retornando Nome, CPF e data de nascimento (usada como senha inicial criptografada).
- `VerificarUsuarioJaCadastrado` — `COUNT(*)` em `PotyIA.Usuarios` por CPF.

#### `RefreshTokenRepositorio`
CRUD da tabela `PotyIA.RefreshTokens`:
- `Inserir` — grava novo refresh token (apenas hash).
- `BuscarPorHash` — recupera a entidade pelo `TokenHash`.
- `Revogar(id, substituidoPorTokenHash)` — marca `RevogadoEm` e o hash substituto (só se ainda não revogado).
- `RevogarTodosDoUsuario(usuarioID)` — revoga todos os tokens ativos do usuário (usado na defesa contra reutilização).

---

### 5.4 Models

| Model | Papel |
|-------|-------|
| `AutenticacaoModel` | Entrada de login (CPF, Senha). |
| `UsuarioAutenticadoModel` | Usuário autenticado (UsuarioID, Nome). |
| `UsuarioFormModel` | Dados para cadastro (Nome, CPF, Senha). |
| `RefreshTokenModel` | Entidade do refresh token; propriedades calculadas `Expirado`, `Revogado`, `Ativo`. |
| `RefreshTokenRequestModel` | Corpo do endpoint de refresh. |
| `TokensRespostaModel` | Retorno padrão de login/refresh (AccessToken, RefreshToken, RefreshTokenExpiraEm). |
| `ResponseModel` | Resposta de erro padronizada (Status, Resposta). |

---

### 5.5 Helpers e Interfaces

- **`Helper : IHelper`** — `Criptografar(string)` gera hash **SHA-256** em hexadecimal. Usado para senhas e para o hash dos refresh tokens (o token puro nunca é persistido).
- **Interfaces/** — contratos (`IHelper`, `IBasicoRepositorio`, `IAutenticacaoRepositorio`, `IUsuarioRepositorio`, `IRefreshTokenRepositorio`) que desacoplam as camadas e permitem a injeção de dependência.

---

## 6. Banco de dados

Dois bancos SQL Server são utilizados (connection strings em `appsettings.json`):

- **`InternosPoty`** (schema `PotyIA`) — dados da aplicação: `PotyIA.Usuarios` e `PotyIA.RefreshTokens`.
- **`Senior`** (`vetorh_prod`) — sistema de RH; leitura de funcionários em `VETORH_PROD.dbo.R034FUN`.

### Tabela `PotyIA.RefreshTokens` (`Database/RefreshTokens.sql`)
| Coluna | Tipo | Observação |
|--------|------|-----------|
| `Id` | BIGINT IDENTITY | PK clusterizada |
| `TokenHash` | VARCHAR(128) | índice **único** — apenas o hash |
| `UsuarioID` | VARCHAR(50) | FK para `PotyIA.Usuarios` |
| `CriadoEm` / `ExpiraEm` | DATETIME2(3) | ciclo de vida |
| `RevogadoEm` | DATETIME2(3) NULL | revogação |
| `SubstituidoPorTokenHash` | VARCHAR(128) NULL | rastreio da rotação |

Índices: `UX_RefreshTokens_TokenHash` (único) e `IX_RefreshTokens_UsuarioID`.

---

## 7. Fluxos ponta a ponta

### 7.1 Cadastro de usuário
```
POST api/Usuario/{cpf}
  → UsuarioController.CriarUsuario
  → UsuarioService.CadastrarUsuario
	  → VerificarUsuarioJaCadastrado (InternosPoty)  → se existe, erro
	  → VerificarUsuarioSenior       (Senior/VETORH) → dados + senha (data nasc.)
	  → CadastrarUsuario             (InternosPoty)  → INSERT
  → 201 Created
```

### 7.2 Login
```
POST api/Autenticacao  { CPF, Senha }
  → AutenticacaoController.RealizarAutenticacao
  → AutenticacaoService.RealizarAutenticacao
	  → AutenticacaoRepositorio (valida CPF + Senha criptografada, Ativo=1)
  → RefreshTokenService.GerarTokens
	  → GerarAccessToken (JWT) + CriarRefreshToken + Inserir
  → { AccessToken, RefreshToken, RefreshTokenExpiraEm, Usuario }
```

### 7.3 Renovação (refresh com rotação)
```
POST api/Autenticacao/refresh-token  { RefreshToken }
  → RefreshTokenService.RenovarTokens
	  → hash → BuscarPorHash
	  → inexistente/expirado → 401 genérico
	  → revogado reapresentado → RevogarTodosDoUsuario + 401
	  → válido → cria novo refresh, insere, revoga o antigo, novo AccessToken
  → { AccessToken, RefreshToken, RefreshTokenExpiraEm }
```

---

## 8. Segurança — pontos principais

- **Senhas e refresh tokens** nunca são armazenados em texto puro (SHA-256).
- **Rotação de refresh token** a cada renovação, com detecção de reutilização (revoga toda a cadeia do usuário).
- **Respostas de refresh sempre genéricas** (401) para não vazar o estado do token.
- **Validação estrita do JWT** (`ClockSkew = Zero`, valida issuer/audience/lifetime/assinatura).
- Falha rápida na inicialização se a configuração JWT estiver ausente.

> ⚠️ **Atenção operacional:** o `appsettings.json` contém credenciais de banco e a chave JWT em texto claro no repositório. Recomenda-se migrar esses segredos para **User Secrets**, **variáveis de ambiente** ou **Azure Key Vault**, e removê-los do controle de versão.

---

## 9. Como executar

1. Garantir acesso aos bancos `InternosPoty` e `Senior` (connection strings em `appsettings.json`).
2. Aplicar o script `Database/RefreshTokens.sql` (cria a tabela se não existir).
3. Restaurar/compilar e executar:
   ```powershell
   dotnet run
   ```
4. Acessar a documentação Swagger em `/swagger` (a raiz `/` redireciona automaticamente).
