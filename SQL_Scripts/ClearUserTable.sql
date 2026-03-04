SELECT TOP (100) *
  FROM [UdemySecurityCourse].[dbo].[AspNetUsers]

SELECT usr.Email, cl.* 
 FROM [UdemySecurityCourse].[dbo].[AspNetUserClaims] cl,
      [UdemySecurityCourse].[dbo].[AspNetUsers] usr
WHERE cl.[UserId] = usr.[Id]

/*

DELETE FROM [UdemySecurityCourse].[dbo].[AspNetUserClaims];
DELETE FROM [UdemySecurityCourse].[dbo].[AspNetUsers];

*/