CREATE PROCEDURE [dbo].[ActualizarDocumentoCloud_pa]
(
    @p_tabla TablaDocumento READONLY
)
As
Set Nocount On

-- CREATE MESSAGE TYPE ServiceBus VALIDATION=NONE;
-- CREATE CONTRACT Recaudacion (ServiceBus SENT BY INITIATOR);
-- CREATE QUEUE SenderCola;
-- CREATE QUEUE RecipCola;
-- CREATE SERVICE SenderServicio ON QUEUE SenderCola(Recaudacion);
-- CREATE SERVICE RecipServicio ON QUEUE RecipCola(Recaudacion);

DECLARE @IdEmpresa SMALLINT, @IdTipodocumento SMALLINT, @NroDocumento CHAR(13)
 
DECLARE CUR_TEST CURSOR FAST_FORWARD FOR
    SELECT IdEmpresa, IdTipodocumento, NroDocumento FROM @p_tabla
 
OPEN CUR_TEST
FETCH NEXT FROM CUR_TEST INTO @IdEmpresa, @IdTipodocumento, @NroDocumento 
 
WHILE @@FETCH_STATUS = 0
BEGIN

	DECLARE @conversation uniqueidentifier, @msg varchar(max)='';
	SELECT @msg = 'IdEmpresa : ' + Cast(@IdEmpresa as varchar) + CHAR(13) 
			+ 'IdTipodocumento : ' + Cast(@IdTipodocumento as varchar) + CHAR(13) 
			+ 'NroDocumento : ' + @NroDocumento

	--- Inicia una Conversacion:
	BEGIN DIALOG @conversation 
		FROM SERVICE SenderServicio
		TO SERVICE N'RecipServicio'
		ON CONTRACT Recaudacion
		WITH ENCRYPTION=OFF;

	--- Envia el Mensaje 
	SEND ON CONVERSATION @conversation MESSAGE TYPE ServiceBus (@msg);

   FETCH NEXT FROM CUR_TEST INTO @IdEmpresa, @IdTipodocumento, @NroDocumento 
END
CLOSE CUR_TEST
DEALLOCATE CUR_TEST


