SET IDENTITY_INSERT BatchSettings ON
GO

INSERT INTO BatchSettings (Id,Period1,Period2,Period3,BankAccName,BIC,IBAN,AuthId,BankJournalCode,TradeJournalCode,BankGlCode,TradeGlCode,ClassificationFilterID,CreatedOn,UpdatedOn,Bank_Id,UpdatedBy_Id)
SELECT Id,Period1,Period2,Period3,BankAccName,BIC,IBAN,AuthId,BankJournalCode,SalesJournalCode, BankGlCode,TradeDebtorGlCode,ClassificationFilterID,CreatedOn,UpdatedOn,BankId,UpdatedBy_Id
FROM Organisation
GO

INSERT INTO BatchSettings (Period1,Period2,Period3,BankAccName,BIC,IBAN,AuthId,BankJournalCode,TradeJournalCode,BankGlCode,TradeGlCode,ClassificationFilterID,CreatedOn,UpdatedOn,Bank_Id,UpdatedBy_Id)
VALUES (30,60,90,'Bank Acc','AIB00000','IEXXAIBXXXXXXXXXXXXXXX','EI00000000000',null,null,null,null,null,GETDATE(),null,1,null)
GO

INSERT INTO BatchSettings (Period1,Period2,Period3,BankAccName,BIC,IBAN,AuthId,BankJournalCode,TradeJournalCode,BankGlCode,TradeGlCode,ClassificationFilterID,CreatedOn,UpdatedOn,Bank_Id,UpdatedBy_Id)
VALUES (30,60,90,'Bank Acc','AIB00000','IEXXAIBXXXXXXXXXXXXXXX','EI00000000000',null,null,null,null,null,GETDATE(),null,1,null)
GO

INSERT INTO BatchSettings (Period1,Period2,Period3,BankAccName,BIC,IBAN,AuthId,BankJournalCode,TradeJournalCode,BankGlCode,TradeGlCode,ClassificationFilterID,CreatedOn,UpdatedOn,Bank_Id,UpdatedBy_Id)
VALUES (30,60,90,'Bank Acc','AIB00000','IEXXAIBXXXXXXXXXXXXXXX','EI00000000000',null,null,null,null,null,GETDATE(),null,1,null)
GO

SET IDENTITY_INSERT BatchSettings OFF
GO

UPDATE Organisation SET DirectDebitSettings_Id = Id
GO