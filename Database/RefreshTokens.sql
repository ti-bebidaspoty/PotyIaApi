-- ============================================================
-- Tabela de Refresh Tokens
-- Banco: InternosPoty  |  Schema: PotyIA
-- Apenas o HASH do refresh token é armazenado (TokenHash),
-- nunca o valor puro entregue ao cliente.
-- ============================================================

IF NOT EXISTS (
	SELECT 1 FROM sys.tables t
	JOIN sys.schemas s ON s.schema_id = t.schema_id
	WHERE s.name = 'PotyIA' AND t.name = 'RefreshTokens'
)
BEGIN
	CREATE TABLE PotyIA.RefreshTokens
	(
		Id                       BIGINT IDENTITY(1,1) NOT NULL,
		TokenHash                VARCHAR(128)         NOT NULL,
		UsuarioID                VARCHAR(50)          NOT NULL,
		CriadoEm                 DATETIME2(3)         NOT NULL,
		ExpiraEm                 DATETIME2(3)         NOT NULL,
		RevogadoEm               DATETIME2(3)         NULL,
		SubstituidoPorTokenHash  VARCHAR(128)         NULL,

		CONSTRAINT PK_RefreshTokens PRIMARY KEY CLUSTERED (Id),

		-- Relacionamento com o usuário. Ajuste o nome da PK caso difira.
		CONSTRAINT FK_RefreshTokens_Usuarios
			FOREIGN KEY (UsuarioID) REFERENCES PotyIA.Usuarios (UsuarioID)
	);

	-- Índice único para busca rápida por hash e para impedir colisões.
	CREATE UNIQUE INDEX UX_RefreshTokens_TokenHash
		ON PotyIA.RefreshTokens (TokenHash);

	-- Índice para consultas/revogações por usuário.
	CREATE INDEX IX_RefreshTokens_UsuarioID
		ON PotyIA.RefreshTokens (UsuarioID);
END
GO
