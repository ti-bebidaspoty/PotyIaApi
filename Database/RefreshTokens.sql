-- ============================================================
-- Tabela de Refresh Tokens
-- Banco: InternosPoty  |  Schema: PotyIA
-- Apenas o HASH do refresh token é armazenado (TokenHash),
-- nunca o valor puro entregue ao cliente.
-- A identidade do usuário vem de Global.Usuarios (PotyInternos).
-- ============================================================

IF OBJECT_ID('PotyIA.RefreshTokens', 'U') IS NULL
BEGIN

	CREATE TABLE PotyIA.RefreshTokens
	(
		Id BIGINT IDENTITY(1,1) NOT NULL,

		TokenHash VARCHAR(128) NOT NULL,

		UsuarioID VARCHAR(50) NOT NULL,

		CriadoEm DATETIME2(3) NOT NULL,

		ExpiraEm DATETIME2(3) NOT NULL,

		RevogadoEm DATETIME2(3) NULL,

		SubstituidoPorTokenHash VARCHAR(128) NULL,

		CONSTRAINT PK_RefreshTokens
			PRIMARY KEY CLUSTERED (Id),

		CONSTRAINT FK_RefreshTokens_GlobalUsuarios
			FOREIGN KEY (UsuarioID)
			REFERENCES Global.Usuarios (UsuarioID)
	);

END;
GO


/* ============================================================
   ÍNDICE ÚNICO PARA TOKEN HASH
   ============================================================ */

IF NOT EXISTS (
	SELECT 1
	FROM sys.indexes
	WHERE name = 'UX_RefreshTokens_TokenHash'
	  AND object_id = OBJECT_ID('PotyIA.RefreshTokens')
)
BEGIN

	CREATE UNIQUE INDEX UX_RefreshTokens_TokenHash
		ON PotyIA.RefreshTokens (TokenHash);

END;
GO


/* ============================================================
   ÍNDICE PARA BUSCAS POR USUÁRIO
   ============================================================ */

IF NOT EXISTS (
	SELECT 1
	FROM sys.indexes
	WHERE name = 'IX_RefreshTokens_UsuarioID'
	  AND object_id = OBJECT_ID('PotyIA.RefreshTokens')
)
BEGIN

	CREATE INDEX IX_RefreshTokens_UsuarioID
		ON PotyIA.RefreshTokens (UsuarioID);

END;
GO
