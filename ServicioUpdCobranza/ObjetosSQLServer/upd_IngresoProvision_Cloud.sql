CREATE TRIGGER [dbo].[upd_IngresoProvision_Cloud] ON [dbo].[IngresoProvision] 
FOR UPDATE 
AS 
SET NOCOUNT ON;

IF (left(app_name(),21) != 'Microsoft JDBC Driver' )
	AND EXISTS( Select idTipoDocumento From inserted Where idTipoDocumento in (7,3, 210) )

BEGIN 
	IF UPDATE(Saldo) OR UPDATE(EstadoDeuda)
	BEGIN 
		DECLARE @Tabla AS TablaDocumento;

		INSERT INTO @Tabla (IdEmpresa, IdTipoDocumento, NroDocumento)
		SELECT idEmpresa, IdTipoDocumento, NroDocumento 
		FROM inserted;

		EXEC dbo.EnviarDocumentoCloud_pa @p_tabla = @Tabla

	END 
END 




