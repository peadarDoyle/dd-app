declare @OrganisationId int = 2

delete from Allocation where Account_Id in (select Id from Account where Organisation_Id = @OrganisationId)
delete from Account where Organisation_Id = @OrganisationId
delete from Batch where Organisation_Id = @OrganisationId
delete from AspNetUserLogins where UserId in (select Id from AspNetUsers where Organisation_Id = @OrganisationId)

update BatchSettings set [CreatedBy_Id] = null, [UpdatedBy_Id] = null 
	where Id = (select DirectDebitSettings_Id from Organisation where Id = @OrganisationId)
	   or Id = (select PaymentSettings_Id from Organisation where Id = @OrganisationId)

update Organisation set [CreatedBy_Id] = null, [UpdatedBy_Id] = null 
	where Id = @OrganisationId

delete from AspNetUsers where Organisation_Id = @OrganisationId
delete from Organisation where Id = @OrganisationId
delete from BatchSettings where Id in (select Id from Batch where Organisation_Id = @OrganisationId)


