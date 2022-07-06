CREATE PROCEDURE [dbo].[RecibirDocumentoCloud_pa]
(
    @RecievedMessage	NVARCHAR(MAX) OUTPUT,
	@MessageType	NVARCHAR(100) OUTPUT
)
As
Set Nocount On

DECLARE @conversation uniqueidentifier, @senderMsgType nvarchar(100), @msg varchar(max);

	WAITFOR (RECEIVE TOP (1) 
		@conversation = conversation_handle, @msg = message_body, @senderMsgType = message_type_name 
	FROM RecipCola); 

	SELECT @RecievedMessage = @msg , @MessageType = @senderMsgType; 

END CONVERSATION @conversation;



