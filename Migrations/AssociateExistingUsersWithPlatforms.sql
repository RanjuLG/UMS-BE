-- SQL Script to Associate Existing Users with Platforms
-- Run this after the AddUserPlatformManyToMany migration

-- First, let's check what users exist
SELECT UserId, UserName, Email FROM Users;

-- Check what platforms exist
SELECT PlatformId, Name FROM Platforms;

-- Check current UserPlatforms (should be empty or have only seed data)
SELECT * FROM UserPlatforms;

-- Associate all existing users with the first platform (UMS Platform)
-- Adjust the PlatformId based on your needs
INSERT INTO UserPlatforms (UserId, PlatformId, CreatedAt, CreatedBy)
SELECT 
    u.UserId,
    1 as PlatformId, -- Change this to the appropriate platform ID
    GETDATE() as CreatedAt,
    NULL as CreatedBy
FROM Users u
WHERE NOT EXISTS (
    SELECT 1 FROM UserPlatforms up 
    WHERE up.UserId = u.UserId AND up.PlatformId = 1
);

-- Verify the associations
SELECT 
    u.UserId,
    u.UserName,
    u.Email,
    p.PlatformId,
    p.Name as PlatformName
FROM Users u
JOIN UserPlatforms up ON u.UserId = up.UserId
JOIN Platforms p ON up.PlatformId = p.PlatformId
ORDER BY u.UserId;
