-- Enable RLS
--CREATE SCHEMA rls -- separate schema to organize RLS objects
--GO

ALTER FUNCTION rls.fn_tenantAccessPredicate(@TenantId int)
    RETURNS TABLE
    WITH SCHEMABINDING
AS
    RETURN SELECT 1 AS fn_accessResult 
        WHERE 
		(
			DATABASE_PRINCIPAL_ID() =  DATABASE_PRINCIPAL_ID('dbo') -- note, should not be dbo!
			AND CAST(SESSION_CONTEXT(N'TenantId') AS int) = @TenantId
		) 
		OR
		(
			SUSER_NAME() = 'ivhadmin2'
		)
GO

--====================================

Drop SECURITY POLICY rls.tenantAccessPolicy

Create SECURITY POLICY rls.tenantAccessPolicy
    ADD FILTER PREDICATE rls.fn_tenantAccessPredicate(TenantId) ON dbo.PROJECT,
	ADD BLOCK PREDICATE rls.fn_tenantAccessPredicate(TenantId) ON dbo.PROJECT

GO

--======================================

ALTER TABLE PROJECT
    ADD CONSTRAINT df_TenantId_PROJECT 
	DEFAULT CAST(SESSION_CONTEXT(N'TenantId') AS int) FOR TenantId
GO

--=======================================
EXEC sp_set_session_context 'TenantId', 5
SELECT SESSION_CONTEXT(N'TenantId');  