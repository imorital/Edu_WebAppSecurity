SELECT TOP (100) * FROM [UdemySecurityCourse].[dbo].[AspNetUsers]

SELECT usr.Email, cl.* 
 FROM [UdemySecurityCourse].[dbo].[AspNetUserClaims] cl,
      [UdemySecurityCourse].[dbo].[AspNetUsers] usr
WHERE cl.[UserId] = usr.[Id]

SELECT usr.Email, rl.* 
 FROM [UdemySecurityCourse].[dbo].[AspNetUserRoles] rl,
      [UdemySecurityCourse].[dbo].[AspNetUsers] usr
WHERE rl.[UserId] = usr.[Id]

SELECT * FROM [UdemySecurityCourse].[dbo].[AspNetRoles];
SELECT * FROM [UdemySecurityCourse].[dbo].[AspNetUserTokens];

/*

DELETE FROM [UdemySecurityCourse].[dbo].[AspNetUserClaims];
DELETE FROM [UdemySecurityCourse].[dbo].[AspNetRoles];
DELETE FROM [UdemySecurityCourse].[dbo].[AspNetUserTokens];
DELETE FROM [UdemySecurityCourse].[dbo].[AspNetUsers];

*/