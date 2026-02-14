INSERT INTO Accounts (UserId,Name,"Type",Balance,Currency,AccountNumber,Institution,Color,Icon,IsActive,IncludeInTotal,CreatedAt,UpdatedAt) VALUES
	 (1,'Chase Checking','Checking',3250.5,'USD',NULL,NULL,'#3B82F6','bank',1,1,'2026-02-10 02:52:13','2026-02-10 02:52:13'),
	 (1,'Cash Wallet','Cash',245.25,'USD',NULL,NULL,'#3B82F6','bank',1,1,'2026-02-10 02:52:13','2026-02-10 02:52:13'),
	 (5,'Cash Wallet','Cash',600.0,'BYN',NULL,NULL,'#3B82F6','wallet',1,1,'2026-02-14 02:25:47.2529231','2026-02-14 02:25:47.2529702'),
	 (5,'Bank Card','Checking',100.0,'BYN',NULL,NULL,'#10B981','credit-card',1,1,'2026-02-14 04:35:14.677163','2026-02-14 04:35:14.6772115');
INSERT INTO Bills (UserId,Name,Amount,CategoryId,DueDay,Frequency,NextDueDate,IsActive,CreatedAt) VALUES
	 (5,'Apartment Rent',800.0,8,1,'Monthly','2026-05-01 00:00:00',1,'2026-02-14 04:14:26.5038218');
INSERT INTO Budgets (UserId,CategoryId,Amount,Period,StartDate,IsActive,CreatedAt) VALUES
	 (5,8,100.0,'Monthly','2026-02-01 00:00:00',1,'2026-02-14 03:51:47.1581578');
INSERT INTO Categories (UserId,Name,"Type",Icon,Color,ParentCategoryId,IsEssential,IsActive,CreatedAt) VALUES
	 (NULL,'Salary','Income','dollar-sign','#10B981',NULL,1,1,'2026-02-10 02:50:32'),
	 (NULL,'Freelance','Income','code','#10B981',NULL,0,1,'2026-02-10 02:50:32'),
	 (NULL,'Rent/Mortgage','Expense','home','#EF4444',NULL,1,1,'2026-02-10 02:50:32'),
	 (NULL,'Groceries','Expense','shopping-cart','#F59E0B',NULL,1,1,'2026-02-10 02:50:32'),
	 (NULL,'Dining Out','Expense','utensils','#8B5CF6',NULL,0,1,'2026-02-10 02:50:32'),
	 (NULL,'Coffee','Expense','coffee','#8B5CF6',NULL,0,1,'2026-02-10 02:50:32'),
	 (NULL,'Transfer','Transfer','refresh-cw','#6B7280',NULL,0,1,'2026-02-10 02:50:32'),
	 (5,'Food & Drink','Expense','restaurant','#F44336',NULL,0,1,'2026-02-14 03:26:41'),
	 (5,'Adjustments','System','sync','#9E9E9E',NULL,0,1,'2026-02-14 05:27:46');
INSERT INTO Transactions (UserId,AccountId,Amount,Description,CategoryId,TransactionDate,CreatedDate,Merchant,PaymentMethod,Notes) VALUES
	 (1,1,-6.5,'Morning Coffee',6,'2024-02-10','2026-02-10 02:52:15','Starbucks',NULL,NULL),
	 (1,1,-45.2,'Weekly Groceries',4,'2024-02-09','2026-02-10 02:52:15','Walmart',NULL,NULL),
	 (1,1,5000.0,'Monthly Salary',1,'2024-02-01','2026-02-10 02:52:15','ABC Corp',NULL,NULL),
	 (5,3,-15.5,'Starbucks - Updated',8,'2026-02-14','2026-02-14 03:29:24',NULL,NULL,NULL),
	 (5,4,-800.0,'Bill Paid: Apartment Rent',8,'2026-02-14','2026-02-14 05:08:20.7867756',NULL,NULL,NULL),
	 (5,3,15.5,'Balance Adjustment (Manual Sync)',9,'2026-02-14','2026-02-14 05:37:59.3637082',NULL,NULL,'Adjusted from 584.5 to 600');
INSERT INTO Users (Username,Email,PasswordHash,FullName,Currency,MonthlyIncome,CreatedAt,UpdatedAt,LastLogin,IsActive,AppPinHash) VALUES
	 ('john','john@email.com','hashed_password_123','John Doe','USD',5000.0,'2026-02-10 02:51:28','2026-02-10 02:51:28',NULL,1,NULL),
	 ('was','user@example.com','$2a$11$bBfWdqYMq4nDFn5RJSJL4O.C1wa9lao4EzG2GvHqUvmIZ8x.dw6cm','string','string',179769313486231570000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000,'2026-02-10 03:45:21.4854525','2026-02-10 03:45:21.4855559','2026-02-10 03:46:15.7401207',1,NULL),
	 ('wasim','wasim@example.com','$2a$11$InF52lP/o/H2gXo.LBSY9ene/d62C3eKLMsvSVoWFu3rOdNVgFUbW','Wasim Younes','BYN',4500.0,'2026-02-14 00:14:31.6569508','2026-02-14 00:14:31.6570128','2026-02-14 01:15:32.6036823',1,NULL),
	 ('wasim1','wasim@example1.com','$2a$11$W9F4VqC9dQX8qKgCIVWJKuTd0igPLICSKTlVG3ZbubsPYpvR7MTQW','Wasim Younes','USD',0.0,'2026-02-14 00:21:01.1520947','2026-02-14 00:21:01.1521807',NULL,1,NULL),
	 ('wasim2','wasim@example2.com','$2a$11$MiLnmoxh9fOhUTOnISgDaeAUZDS5yfbL7/LGC1X77VUmnZB9NrXki','Wasim Younes','USD',4500.0,'2026-02-14 01:52:13.0852577','2026-02-14 01:52:13.0853163','2026-02-14 05:23:26.8630303',1,'$2a$11$L/J3zEeLytP7eaxwIIirJOAADHpiIjwzeqcunSJfjNx9tKudNV3ZS');
