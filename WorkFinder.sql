INSERT INTO "users" (
    "Id",
    "UserName",
    "NormalizedUserName",
    "Email",
    "NormalizedEmail",
    "EmailConfirmed",
    "PasswordHash",
    "SecurityStamp",
    "ConcurrencyStamp",
    "PhoneNumber",
    "PhoneNumberConfirmed",
    "TwoFactorEnabled",
    "LockoutEnd",
    "LockoutEnabled",
    "AccessFailedCount",
    "FirstName",
    "LastName",
    "ProfilePicture",
    "DateOfBirth"
)
VALUES
-- User 1: Parks America
(
    '1', -- ID tuần tự thay vì UUID
    'parks.america@example.com',
    'PARKS.AMERICA@EXAMPLE.COM',
    'parks.america@example.com',
    'PARKS.AMERICA@EXAMPLE.COM',
    TRUE, -- Email đã xác thực
    'AQAAAAIAAYagAAAAELTKZ2qnWbhTMDpscMcSNNLqPDYL2Svvhj1mJ5j2+1MdZqW5rVXSqjgokgLdBizfKw==', -- Mật khẩu mã hóa
    'f47ac10b-58cc-4372-a567-0e02b2c3d479', -- SecurityStamp cố định
    'f47ac10b-58cc-4372-a567-0e02b2c3d479', -- ConcurrencyStamp cố định
    '0123456789',
    FALSE, -- Chưa xác thực số điện thoại
    FALSE, -- Không bật 2FA
    NULL, -- Không bị khóa
    TRUE, -- Cho phép khóa tài khoản
    0, -- Không có lần đăng nhập thất bại
    'John', -- FirstName
    'Parks', -- LastName
    'https://img.logo.dev/animalsafari.com?token=pk_JNtYBCy6RyOkevRWc-7ghw', -- ProfilePicture từ logo_url
    '1985-05-15'::date -- DateOfBirth với kiểu date của PostgreSQL
),

-- User 2: Animal Humane Society
(
    '2',
    'humane.society@example.com',
    'HUMANE.SOCIETY@EXAMPLE.COM',
    'humane.society@example.com',
    'HUMANE.SOCIETY@EXAMPLE.COM',
    TRUE,
    'AQAAAAIAAYagAAAAEJFvAsdXDtGPU5UB/yz6Sd9OvmjpTijRcOlkSPp95bA+Zk49OzKbnUgI+y5L75d+TA==',
    'f47ac10b-58cc-4372-a567-0e02b2c3d480',
    'f47ac10b-58cc-4372-a567-0e02b2c3d480',
    '0123456788',
    FALSE,
    FALSE,
    NULL,
    TRUE,
    0,
    'Sarah',
    'Humane',
    'https://img.logo.dev/animalhumanesociety.org?token=pk_JNtYBCy6RyOkevRWc-7ghw',
    '1990-08-21'::date
),

-- User 3: Animalis
(
    '3',
    'animalis.user@example.com',
    'ANIMALIS.USER@EXAMPLE.COM',
    'animalis.user@example.com',
    'ANIMALIS.USER@EXAMPLE.COM',
    TRUE,
    'AQAAAAIAAYagAAAAEMX9a2DRy4Shs0Kw+wHHQPsZY8RXNaBbcKJJFv9Zj3/9Cp+UpVPaMeTl8W7nrwV+mQ==',
    'f47ac10b-58cc-4372-a567-0e02b2c3d481',
    'f47ac10b-58cc-4372-a567-0e02b2c3d481',
    '0123456787',
    FALSE,
    FALSE,
    NULL,
    TRUE,
    0,
    'Luis',
    'Animalis',
    'https://img.logo.dev/animalis.com?token=pk_JNtYBCy6RyOkevRWc-7ghw',
    '1988-12-03'::date
),

-- User 4: Animal Logic
(
    '4',
    'logic.animal@example.com',
    'LOGIC.ANIMAL@EXAMPLE.COM',
    'logic.animal@example.com',
    'LOGIC.ANIMAL@EXAMPLE.COM',
    TRUE,
    'AQAAAAIAAYagAAAAEHuB5qYzG7uV9MPlxh5oIznO4LQQeJKdI5xN2qPxWcF1X9tvy/4jX4vYAJn5kSfAGA==',
    'f47ac10b-58cc-4372-a567-0e02b2c3d482',
    'f47ac10b-58cc-4372-a567-0e02b2c3d482',
    '0123456786',
    FALSE,
    FALSE,
    NULL,
    TRUE,
    0,
    'Robert',
    'Logic',
    'https://img.logo.dev/animallogic.com?token=pk_JNtYBCy6RyOkevRWc-7ghw',
    '1992-06-17'::date
),

-- User 5: Animal Logic Film
(
    '5',
    'logic.film@example.com',
    'LOGIC.FILM@EXAMPLE.COM',
    'logic.film@example.com',
    'LOGIC.FILM@EXAMPLE.COM',
    TRUE,
    'AQAAAAIAAYagAAAAEG5TktHnRDZrRVu97Q3CLN41yQyVCXQgOBYSXYvXzg8dCNRvGXj/KJ8HXhz/mjF5Cw==',
    'f47ac10b-58cc-4372-a567-0e02b2c3d483',
    'f47ac10b-58cc-4372-a567-0e02b2c3d483',
    '0123456785',
    FALSE,
    FALSE,
    NULL,
    TRUE,
    0,
    'Emma',
    'Film',
    'https://img.logo.dev/al.com.au?token=pk_JNtYBCy6RyOkevRWc-7ghw',
    '1986-09-30'::date
),

-- User 6: Animalz
(
    '6',
    'animalz.co@example.com',
    'ANIMALZ.CO@EXAMPLE.COM',
    'animalz.co@example.com',
    'ANIMALZ.CO@EXAMPLE.COM',
    TRUE,
    'AQAAAAIAAYagAAAAEJ9ZvBxQJDLDbKQs1rPwj5SYuMj43/bJMXBB9iI3GQdJbnW39N62LlGwkULkGOa17g==',
    'f47ac10b-58cc-4372-a567-0e02b2c3d484',
    'f47ac10b-58cc-4372-a567-0e02b2c3d484',
    '0123456784',
    FALSE,
    FALSE,
    NULL,
    TRUE,
    0,
    'Dave',
    'Animalz',
    'https://img.logo.dev/animalz.co?token=pk_JNtYBCy6RyOkevRWc-7ghw',
    '1995-02-14'::date
),

-- User 7: STILETTO Television
(
    '7',
    'stiletto.tv@example.com',
    'STILETTO.TV@EXAMPLE.COM',
    'stiletto.tv@example.com',
    'STILETTO.TV@EXAMPLE.COM',
    TRUE,
    'AQAAAAIAAYagAAAAEKq4g53rBkYlvxFtcw5+1CeVUO2gEZ5YkWWgCdPxFyLmQUO8ZtakvCadOBwLn5HSlA==',
    'f47ac10b-58cc-4372-a567-0e02b2c3d485',
    'f47ac10b-58cc-4372-a567-0e02b2c3d485',
    '0123456783',
    FALSE,
    FALSE,
    NULL,
    TRUE,
    0,
    'Michael',
    'Stiletto',
    'https://img.logo.dev/animalplanet.com?token=pk_JNtYBCy6RyOkevRWc-7ghw',
    '1983-11-09'::date
),

-- User 8: Animal Charity Evaluators
(
    '8',
    'charity.eval@example.com',
    'CHARITY.EVAL@EXAMPLE.COM',
    'charity.eval@example.com',
    'CHARITY.EVAL@EXAMPLE.COM',
    TRUE,
    'AQAAAAIAAYagAAAAEF9CnKlDpLh+KgJvGDgPXSA7ywINIvKNK9/BzxmcKsX+ZxK85ZcS+KDqAvx+lOsz8g==',
    'f47ac10b-58cc-4372-a567-0e02b2c3d486',
    'f47ac10b-58cc-4372-a567-0e02b2c3d486',
    '0123456782',
    FALSE,
    FALSE,
    NULL,
    TRUE,
    0,
    'Jessica',
    'Charity',
    'https://img.logo.dev/animalcharityevaluators.org?token=pk_JNtYBCy6RyOkevRWc-7ghw',
    '1991-04-25'::date
),

-- User 9: ANIMAL Hospital
(
    '9',
    'hospital.animal@example.com',
    'HOSPITAL.ANIMAL@EXAMPLE.COM',
    'hospital.animal@example.com',
    'HOSPITAL.ANIMAL@EXAMPLE.COM',
    TRUE,
    'AQAAAAIAAYagAAAAEKS+HLNd6XRgXZE6/cXMGtT6Jm00+fkLvnmP6zHtJjnXnR+kYJ9eFGfUqfmM06UiQQ==',
    'f47ac10b-58cc-4372-a567-0e02b2c3d487',
    'f47ac10b-58cc-4372-a567-0e02b2c3d487',
    '0123456781',
    FALSE,
    FALSE,
    NULL,
    TRUE,
    0,
    'Laura',
    'Hospital',
    'https://img.logo.dev/animalhospitalcarolinabeach.com?token=pk_JNtYBCy6RyOkevRWc-7ghw',
    '1987-07-12'::date
),

-- User 10: ANIMAL Veterinary
(
    '10',
    'vete.animal@example.com',
    'VETE.ANIMAL@EXAMPLE.COM',
    'vete.animal@example.com',
    'VETE.ANIMAL@EXAMPLE.COM',
    TRUE,
    'AQAAAAIAAYagAAAAEMZ3b5iLgC7WnH3U2DaBOlfQvw4QJYKhKJWMGszKWUiUGRrkgJO+2YkXXzGZMuiJGQ==',
    'f47ac10b-58cc-4372-a567-0e02b2c3d488',
    'f47ac10b-58cc-4372-a567-0e02b2c3d488',
    '0123456780',
    FALSE,
    FALSE,
    NULL,
    TRUE,
    0,
    'Carlos',
    'Veterinary',
    'https://img.logo.dev/animal-vete.com?token=pk_JNtYBCy6RyOkevRWc-7ghw',
    '1993-10-05'::date
);


-- Creating INSERT statements for the Categories table
-- Using sequential IDs from 1-10 
-- BaseEntity provides fields: Id, CreatedAt, UpdatedAt

INSERT INTO "categories" (
    "Id",
    "Name",
    "Description",
    "CreatedAt",
    "UpdatedAt"
)
VALUES
-- Category 1: Information Technology
(
    1,
    'Information Technology',
    'Jobs related to software development, hardware engineering, system administration, cybersecurity, and other technology fields.',
    CURRENT_TIMESTAMP,
    NULL
),

-- Category 2: Business & Marketing
(
    2,
    'Business & Marketing',
    'Positions in sales, business development, advertising, marketing, public relations, and market analysis.',
    CURRENT_TIMESTAMP,
    NULL
),

-- Category 3: Finance & Accounting
(
    3,
    'Finance & Accounting',
    'Jobs related to accounting, taxation, auditing, banking, insurance, and financial management.',
    CURRENT_TIMESTAMP,
    NULL
),

-- Category 4: Healthcare & Pharmaceuticals
(
    4,
    'Healthcare & Pharmaceuticals',
    'Positions in healthcare, hospitals, pharmaceuticals, medical research, and medical devices.',
    CURRENT_TIMESTAMP,
    NULL
),

-- Category 5: Education & Training
(
    5,
    'Education & Training',
    'Jobs in teaching, training, educational administration, and curriculum development.',
    CURRENT_TIMESTAMP,
    NULL
),

-- Category 6: Media & Entertainment
(
    6,
    'Media & Entertainment',
    'Positions related to film, television, music, journalism, digital media, and entertainment production.',
    CURRENT_TIMESTAMP,
    NULL
),

-- Category 7: Engineering & Manufacturing
(
    7,
    'Engineering & Manufacturing',
    'Jobs in civil engineering, mechanical engineering, electrical engineering, manufacturing, and production.',
    CURRENT_TIMESTAMP,
    NULL
),

-- Category 8: Hospitality & Tourism
(
    8,
    'Hospitality & Tourism',
    'Positions in hotels, restaurants, travel agencies, tourism management, and event planning.',
    CURRENT_TIMESTAMP,
    NULL
),

-- Category 9: Legal Services
(
    9,
    'Legal Services',
    'Jobs related to law practice, legal consulting, compliance, and regulatory affairs.',
    CURRENT_TIMESTAMP,
    NULL
),

-- Category 10: Human Resources
(
    10,
    'Human Resources',
    'Positions in recruitment, talent management, employee relations, compensation and benefits, and organizational development.',
    CURRENT_TIMESTAMP,
    NULL
);

INSERT INTO "resumes" (
    "Id",
    "Summary",
    "Skills",
    "Education",
    "Experience",
    "Certifications",
    "Languages",
    "FileUrl",
    "UserId",
    "CreatedAt",
    "UpdatedAt"
)
VALUES
-- Resume for User 1: John Parks (Parks America)
(
    1,
    'Experienced wildlife management professional with over 10 years of experience in animal conservation and park management. Passionate about creating educational and engaging experiences for visitors while ensuring animal welfare.',
    'Wildlife Management, Conservation Planning, Team Leadership, Budget Management, Public Speaking, Educational Program Development',
    'Bachelor of Science in Wildlife Biology, University of Montana, 2007
     Master of Science in Conservation Biology, Colorado State University, 2009',
    'Director of Operations, Parks America, 2015-Present
     Wildlife Manager, National Park Service, 2009-2015
     Conservation Intern, World Wildlife Fund, 2008',
    'Certified Wildlife Manager (CWM)
     Advanced First Aid and CPR
     Protected Area Management Certification',
    'English (Native), Spanish (Intermediate)',
    '/resumes/john_parks_resume.pdf',
    1,
    CURRENT_TIMESTAMP,
    NULL
),

-- Resume for User 2: Sarah Humane (Animal Humane Society)
(
    2,
    'Dedicated animal welfare advocate with expertise in shelter management and animal rehabilitation. Committed to improving the lives of animals through education, proper care, and finding forever homes.',
    'Animal Care, Shelter Management, Volunteer Coordination, Fundraising, Community Outreach, Animal Behavior Assessment',
    'Bachelor of Science in Animal Science, University of Minnesota, 2012
     Certificate in Nonprofit Management, Metropolitan State University, 2014',
    'Executive Director, Animal Humane Society, 2018-Present
     Shelter Manager, City Animal Shelter, 2014-2018
     Veterinary Assistant, All Creatures Veterinary Clinic, 2012-2014',
    'Certified Animal Welfare Administrator (CAWA)
     Fear Free Shelter Professional Certification
     Humane Education Specialist',
    'English (Native), French (Basic)',
    '/resumes/sarah_humane_resume.pdf',
    2,
    CURRENT_TIMESTAMP,
    NULL
),

-- Resume for User 8: Jessica Charity (Animal Charity Evaluators)
(
    3,
    'Analytical researcher with strong background in program evaluation and impact assessment. Specialized in evaluating animal welfare programs and helping donors make informed decisions about charitable giving.',
    'Research Design, Statistical Analysis, Program Evaluation, Grant Writing, Data Visualization, Impact Assessment, Nonprofit Management',
    'Bachelor of Arts in Economics, Yale University, 2013
     Master of Public Policy, University of California, Berkeley, 2015',
    'Research Director, Animal Charity Evaluators, 2019-Present
     Program Analyst, GiveWell, 2015-2019
     Research Assistant, Poverty Action Lab, 2013-2015',
    'Certified Nonprofit Professional (CNP)
     Data Science Certificate, Coursera
     Project Management Professional (PMP)',
    'English (Native), Mandarin (Conversational), German (Basic)',
    '/resumes/jessica_charity_resume.pdf',
    8,
    CURRENT_TIMESTAMP,
    NULL
),

-- Resume for User 9: Laura Hospital (ANIMAL Hospital)
(
    4,
    'Experienced veterinary professional with expertise in small animal medicine and hospital administration. Dedicated to providing exceptional care to animals and supporting pet owners through education and compassionate service.',
    'Veterinary Medicine, Hospital Management, Staff Training, Client Communication, Emergency Care, Surgical Assistance, Electronic Medical Records',
    'Doctor of Veterinary Medicine, North Carolina State University, 2014
     Bachelor of Science in Biology, University of North Carolina, 2010',
    'Hospital Director, ANIMAL Hospital, 2018-Present
     Lead Veterinarian, Carolina Pet Clinic, 2014-2018
     Veterinary Intern, NC State Veterinary Teaching Hospital, 2013-2014',
    'American Veterinary Medical Association (AVMA) Member
     Fear Free Certified Professional
     Advanced Cardiac Life Support for Veterinary Professionals',
    'English (Native), Spanish (Professional Working Proficiency)',
    '/resumes/laura_hospital_resume.pdf',
    9,
    CURRENT_TIMESTAMP,
    NULL
);


-- Insert Google-related companies with all BaseEntity fields and unique OwnerId values
INSERT INTO public."companies" (
    "Id", 
    "Name", 
    "Description", 
    "Logo",
    "Banner",
    "CategoryId",
    "Industry",
    "EmployeeCount",
    "FoundedDate",
    "Website",
    "Vision",
    "Location",
    "Phone",
    "Email",
    "IsVerified",
    "OwnerId",
    "CreatedAt",
    "UpdatedAt"
)
VALUES
    (1, 'Google', 'Technology company specializing in internet-related services and products.', 
    'https://img.logo.dev/google.com?token=pk_JNtYBCy6RyOkevRWc-7ghw',
    '/uploads/banners/default-banner.jpg',
    1,
    'Technology',
    150000,
    '1998-09-04',
    'https://google.com',
    'To organize the world''s information and make it universally accessible and useful.',
    'Mountain View, CA',
    '+1 650-253-0000',
    'press@google.com',
    true, 
    1, 
    CURRENT_TIMESTAMP, 
    NULL),

    (2, 'Google Cloud', 'About Google corporate information.', 
    'https://img.logo.dev/about.google?token=pk_JNtYBCy6RyOkevRWc-7ghw',
    '/uploads/banners/default-banner.jpg',
    1,
    'Technology',
    150000,
    '2008-04-07',
    'https://about.google',
    'To accelerate organizations'' ability to digitally transform with the best infrastructure, platform, industry solutions and expertise.',
    'Mountain View, CA',
    '+1 650-253-0000',
    'cloud-press@google.com',
    true, 
    2, 
    CURRENT_TIMESTAMP, 
    NULL),

    (3, 'Latest News', 'Google Cloud press information and news.', 
    'https://img.logo.dev/googlecloudpresscorner.com?token=pk_JNtYBCy6RyOkevRWc-7ghw',
    '/uploads/banners/default-banner.jpg',
    2,
    'Media',
    50,
    '2020-01-01',
    'https://googlecloudpresscorner.com',
    'Delivering the latest news and updates about Google Cloud.',
    'Unknown',
    '0867713501',
    'news@googlecloud.com',
    false, 
    3, 
    CURRENT_TIMESTAMP, 
    NULL),

    (4, 'Google Map SEO Service', 'SEO service specializing in Google Maps optimization.', 
    'https://img.logo.dev/googlemapseo.com?token=pk_JNtYBCy6RyOkevRWc-7ghw',
    '/uploads/banners/default-banner.jpg',
    3,
    'Marketing',
    25,
    '2018-06-15',
    'https://googlemapseo.com',
    'Helping businesses maximize their visibility on Google Maps.',
    'Unknown',
    '+1 555-0123',
    'contact@googlemapseo.com',
    false, 
    4, 
    CURRENT_TIMESTAMP, 
    NULL),

    (5, 'Autocompete', 'Google Feud game platform.', 
    'https://img.logo.dev/googlefeud.com?token=pk_JNtYBCy6RyOkevRWc-7ghw',
    '/uploads/banners/default-banner.jpg',
    4,
    'Gaming',
    10,
    '2019-03-20',
    'https://googlefeud.com',
    'Creating engaging gaming experiences based on search trends.',
    'Unknown',
    '0867713501',
    'support@googlefeud.com',
    false, 
    5, 
    CURRENT_TIMESTAMP, 
    NULL),

    (6, 'Google DeepMind', 'AI research laboratory acquired by Google.', 
    'https://img.logo.dev/deepmind.google?token=pk_JNtYBCy6RyOkevRWc-7ghw',
    '/uploads/banners/default-banner.jpg',
    1,
    'Artificial Intelligence',
    1000,
    '2010-09-23',
    'https://deepmind.google',
    'Solving intelligence to advance science and benefit humanity.',
    'London, UK',
    '+44 20 3873 1200',
    'press@deepmind.com',
    true, 
    6, 
    CURRENT_TIMESTAMP, 
    NULL),

    (7, 'Google Ads Mastermind', 'Google Ads expertise and services.', 
    'https://img.logo.dev/googleads.com?token=pk_JNtYBCy6RyOkevRWc-7ghw',
    '/uploads/banners/default-banner.jpg',
    3,
    'Digital Marketing',
    500,
    '2015-11-30',
    'https://googleads.com',
    'Empowering businesses to succeed in digital advertising.',
    'Unknown',
    '+1 555-0189',
    'support@googleadsmastermind.com',
    true, 
    7, 
    CURRENT_TIMESTAMP, 
    NULL),

    (8, 'Google Artificial Intelligence', 'Google''s AI research and products division.', 
    'https://img.logo.dev/ai.google?token=pk_JNtYBCy6RyOkevRWc-7ghw',
    '/uploads/banners/default-banner.jpg',
    1,
    'Artificial Intelligence',
    2000,
    '2016-01-15',
    'https://ai.google',
    'Advancing AI research and applications for a better future.',
    'Mountain View, CA',
    '+1 650-253-0000',
    'ai-press@google.com',
    true, 
    8, 
    CURRENT_TIMESTAMP, 
    NULL),

    (9, 'googlecomplaint.com', 'Platform for submitting complaints about Google services.', 
    'https://img.logo.dev/googlecomplaint.com?token=pk_JNtYBCy6RyOkevRWc-7ghw',
    '/uploads/banners/default-banner.jpg',
    5,
    'Customer Service',
    5,
    '2021-04-01',
    'https://googlecomplaint.com',
    'Providing a voice to users for service improvement.',
    'Unknown',
    '123456789',
    'support@googlecomplaint.com',
    false, 
    9, 
    CURRENT_TIMESTAMP, 
    NULL);

-- Reset the sequence to continue after our manually inserted IDs
SELECT setval('public."companies_Id_seq"', 9, true);



-- Insert 24 job records across the 9 existing companies
INSERT INTO public."jobs" (
    "Id", 
    "Title", 
    "Description", 
    "Requirements", 
    "Benefits", 
    "Location", 
    "SalaryMin", 
    "SalaryMax", 
    "JobType", 
    "ExperienceLevel", 
    "ExpiryDate", 
    "IsActive", 
    "CompanyId", 
    "CreatedAt", 
    "UpdatedAt"
)
VALUES
-- Google (Company ID 1) - 3 jobs
(1, 'Senior Software Engineer', 
   'Join our team to develop cutting-edge technologies that impact billions of users worldwide.', 
   'Strong experience with Java, Python, or Go. 5+ years of software development experience. BS degree in Computer Science or related field.', 
   'Competitive salary, comprehensive health benefits, 401(k) matching, flexible work arrangements, and free meals.', 
   'Mountain View, CA', 
   130000, 180000, 
   0, -- FullTime 
   2, -- Senior
   CURRENT_DATE + INTERVAL '30 days', 
   true, 
   1, 
   CURRENT_TIMESTAMP, NULL),
   
(2, 'Product Manager', 
   'Lead product development for Google''s innovative services.', 
   'Experience in product management. Strong analytical and communication skills. BA/BS degree or equivalent practical experience.', 
   'Competitive salary, health insurance, retirement plan, and various Google perks.', 
   'Mountain View, CA', 
   120000, 160000, 
   0, -- FullTime
   1, -- Intermediate
   CURRENT_DATE + INTERVAL '45 days', 
   true, 
   1, 
   CURRENT_TIMESTAMP, NULL),
   
(3, 'Data Scientist', 
   'Apply statistical analysis and machine learning to solve complex problems at Google.', 
   'MS or PhD in Computer Science, Statistics, or related field. Experience with data analysis and machine learning frameworks.', 
   'Competitive salary, health benefits, stock options, and professional development opportunities.', 
   'Mountain View, CA', 
   140000, 190000, 
   0, -- FullTime
   2, -- Senior
   CURRENT_DATE + INTERVAL '60 days', 
   true, 
   1, 
   CURRENT_TIMESTAMP, NULL),

-- Google (Company ID 2) - 3 jobs
(4, 'UX Designer', 
   'Design user experiences for Google products that are used by millions of people.', 
   'Experience in UX design. Portfolio showcasing your design process. Degree in Design, HCI, or related field.', 
   'Competitive compensation, health benefits, stock options, and work-life balance.', 
   'San Francisco, CA', 
   110000, 150000, 
   0, -- FullTime
   1, -- Intermediate
   CURRENT_DATE + INTERVAL '60 days', 
   true, 
   2, 
   CURRENT_TIMESTAMP, NULL),
   
(5, 'Technical Writer', 
   'Create clear, concise documentation for Google products and services.', 
   'Experience in technical writing. Strong writing and editing skills. Familiarity with technical concepts.', 
   'Competitive salary, health benefits, and professional development opportunities.', 
   'Mountain View, CA', 
   90000, 120000, 
   0, -- FullTime
   1, -- Intermediate
   CURRENT_DATE + INTERVAL '45 days', 
   true, 
   2, 
   CURRENT_TIMESTAMP, NULL),
   
(6, 'Corporate Communications Specialist', 
   'Develop and implement communications strategies for Google.', 
   'Experience in corporate communications or public relations. Strong writing and presentation skills.', 
   'Competitive salary, health benefits, and professional growth opportunities.', 
   'Mountain View, CA', 
   95000, 125000, 
   0, -- FullTime
   1, -- Intermediate
   CURRENT_DATE + INTERVAL '30 days', 
   true, 
   2, 
   CURRENT_TIMESTAMP, NULL),

-- Latest News (Company ID 3) - 2 jobs
(7, 'Technology Reporter', 
   'Cover breaking news and developments in the technology sector, with a focus on Google Cloud.', 
   'Experience in technology journalism. Strong writing and research skills. Degree in Journalism or related field.', 
   'Competitive salary, health insurance, and professional development opportunities.', 
   'New York, NY', 
   70000, 90000, 
   0, -- FullTime
   1, -- Intermediate
   CURRENT_DATE + INTERVAL '30 days', 
   true, 
   3, 
   CURRENT_TIMESTAMP, NULL),
   
(8, 'Content Editor', 
   'Edit and manage content for our technology news platform.', 
   'Experience in editing and content management. Strong attention to detail. Familiarity with technology topics.', 
   'Competitive salary, health benefits, and flexible work arrangements.', 
   'Remote', 
   65000, 85000, 
   2, -- Contract
   1, -- Intermediate
   CURRENT_DATE + INTERVAL '45 days', 
   true, 
   3, 
   CURRENT_TIMESTAMP, NULL),

-- Google Map SEO Service (Company ID 4) - 2 jobs
(9, 'SEO Specialist', 
   'Optimize clients'' Google Maps listings to improve visibility and traffic.', 
   'Experience in SEO, particularly local SEO. Knowledge of Google Maps and Google My Business. Analytical mindset.', 
   'Competitive salary, performance bonuses, and professional development opportunities.', 
   'Remote', 
   60000, 80000, 
   0, -- FullTime
   1, -- Intermediate
   CURRENT_DATE + INTERVAL '30 days', 
   true, 
   4, 
   CURRENT_TIMESTAMP, NULL),
   
(10, 'Local Marketing Consultant', 
   'Advise businesses on optimizing their local online presence, with a focus on Google Maps.', 
   'Experience in local marketing or SEO. Strong communication and client management skills.', 
   'Competitive salary, commission structure, and flexible work arrangements.', 
   'Remote', 
   55000, 75000, 
   0, -- FullTime
   0, -- Entry
   CURRENT_DATE + INTERVAL '45 days', 
   true, 
   4, 
   CURRENT_TIMESTAMP, NULL),

-- Autocompete (Company ID 5) - 2 jobs
(11, 'Game Developer', 
   'Develop and maintain our Google Feud game platform.', 
   'Experience in game development. Proficiency in JavaScript and web technologies. Creative problem-solving skills.', 
   'Competitive salary, health benefits, and creative work environment.', 
   'Remote', 
   80000, 110000, 
   0, -- FullTime
   1, -- Intermediate
   CURRENT_DATE + INTERVAL '60 days', 
   true, 
   5, 
   CURRENT_TIMESTAMP, NULL),
   
(12, 'UI/UX Designer for Gaming', 
   'Design engaging user interfaces for our online game platform.', 
   'Experience in UI/UX design, preferably for games. Portfolio showcasing your design work. Creative mindset.', 
   'Competitive salary, health benefits, and flexible work arrangements.', 
   'Remote', 
   75000, 95000, 
   3, -- Freelance
   1, -- Intermediate
   CURRENT_DATE + INTERVAL '30 days', 
   true, 
   5, 
   CURRENT_TIMESTAMP, NULL),

-- Google DeepMind (Company ID 6) - 4 jobs
(13, 'AI Research Scientist', 
   'Conduct cutting-edge research in artificial intelligence and machine learning.', 
   'PhD in Computer Science, Machine Learning, or related field. Publications in top-tier conferences. Experience with deep learning frameworks.', 
   'Competitive salary, research budget, conference attendance, and publication opportunities.', 
   'London, UK', 
   140000, 200000, 
   0, -- FullTime
   3, -- Executive
   CURRENT_DATE + INTERVAL '90 days', 
   true, 
   6, 
   CURRENT_TIMESTAMP, NULL),
   
(14, 'Machine Learning Engineer', 
   'Implement and scale machine learning models for real-world applications.', 
   'Experience in machine learning engineering. Proficiency in Python and ML frameworks. MS/PhD in Computer Science or related field.', 
   'Competitive salary, health benefits, and professional development opportunities.', 
   'London, UK', 
   120000, 170000, 
   0, -- FullTime
   2, -- Senior
   CURRENT_DATE + INTERVAL '60 days', 
   true, 
   6, 
   CURRENT_TIMESTAMP, NULL),
   
(15, 'Research Intern - AI Ethics', 
   'Contribute to research on ethical considerations in artificial intelligence.', 
   'Currently pursuing MS/PhD in Computer Science, Philosophy, or related field. Interest in AI ethics. Strong analytical skills.', 
   'Competitive stipend, mentorship, and potential for full-time employment.', 
   'London, UK', 
   50000, 60000, 
   4, -- Internship
   0, -- Entry
   CURRENT_DATE + INTERVAL '45 days', 
   true, 
   6, 
   CURRENT_TIMESTAMP, NULL),
   
(16, 'Technical Program Manager - AI Research', 
   'Manage and coordinate AI research programs at Google DeepMind.', 
   'Experience in program management, preferably in research settings. Strong organizational and communication skills. Technical background.', 
   'Competitive salary, health benefits, and professional growth opportunities.', 
   'London, UK', 
   110000, 150000, 
   0, -- FullTime
   2, -- Senior
   CURRENT_DATE + INTERVAL '30 days', 
   true, 
   6, 
   CURRENT_TIMESTAMP, NULL),

-- Google Ads Mastermind (Company ID 7) - 3 jobs
(17, 'Digital Marketing Specialist', 
   'Develop and implement digital marketing strategies for Google Ads clients.', 
   'Experience in digital marketing. Knowledge of SEO, SEM, and social media marketing. BA/BS degree in Marketing or related field.', 
   'Competitive salary, health benefits, professional development opportunities, and flexible work arrangements.', 
   'New York, NY', 
   80000, 110000, 
   0, -- FullTime
   1, -- Intermediate
   CURRENT_DATE + INTERVAL '30 days', 
   true, 
   7, 
   CURRENT_TIMESTAMP, NULL),
   
(18, 'PPC Campaign Manager', 
   'Manage pay-per-click advertising campaigns for clients using Google Ads.', 
   'Experience in PPC advertising. Google Ads certification. Strong analytical and client management skills.', 
   'Competitive salary, performance bonuses, and professional development opportunities.', 
   'Chicago, IL', 
   75000, 95000, 
   0, -- FullTime
   1, -- Intermediate
   CURRENT_DATE + INTERVAL '45 days', 
   true, 
   7, 
   CURRENT_TIMESTAMP, NULL),
   
(19, 'Google Ads Trainer', 
   'Develop and deliver training programs on Google Ads for clients and team members.', 
   'Experience with Google Ads. Strong presentation and teaching skills. Google Ads certification.', 
   'Competitive salary, health benefits, and professional growth opportunities.', 
   'Remote', 
   70000, 90000, 
   1, -- PartTime
   2, -- Senior
   CURRENT_DATE + INTERVAL '60 days', 
   true, 
   7, 
   CURRENT_TIMESTAMP, NULL),

-- Google Artificial Intelligence (Company ID 8) - 3 jobs
(20, 'AI Product Manager', 
   'Lead product development for Google''s AI-powered services.', 
   'Experience in product management, preferably for AI products. Technical background. Strong leadership skills.', 
   'Competitive salary, health benefits, stock options, and professional development opportunities.', 
   'Mountain View, CA', 
   130000, 180000, 
   0, -- FullTime
   2, -- Senior
   CURRENT_DATE + INTERVAL '45 days', 
   true, 
   8, 
   CURRENT_TIMESTAMP, NULL),
   
(21, 'Natural Language Processing Engineer', 
   'Develop and improve NLP models for Google''s AI applications.', 
   'Experience in NLP. Proficiency in Python and deep learning frameworks. MS/PhD in Computer Science or related field.', 
   'Competitive salary, health benefits, and research opportunities.', 
   'Mountain View, CA', 
   140000, 190000, 
   0, -- FullTime
   2, -- Senior
   CURRENT_DATE + INTERVAL '60 days', 
   true, 
   8, 
   CURRENT_TIMESTAMP, NULL),
   
(22, 'AI Ethics Researcher', 
   'Research and develop guidelines for ethical AI development and deployment.', 
   'Background in ethics, philosophy, or related field. Understanding of AI technologies. Strong research and communication skills.', 
   'Competitive salary, health benefits, and publication opportunities.', 
   'San Francisco, CA', 
   120000, 160000, 
   0, -- FullTime
   2, -- Senior
   CURRENT_DATE + INTERVAL '30 days', 
   true, 
   8, 
   CURRENT_TIMESTAMP, NULL),

-- googlecomplaint.com (Company ID 9) - 2 jobs
(23, 'Customer Support Specialist', 
   'Handle customer complaints and inquiries related to Google services.', 
   'Experience in customer service. Strong communication and problem-solving skills. Patience and empathy.', 
   'Competitive salary, health benefits, and professional development opportunities.', 
   'Remote', 
   50000, 65000, 
   0, -- FullTime
   0, -- Entry
   CURRENT_DATE + INTERVAL '30 days', 
   true, 
   9, 
   CURRENT_TIMESTAMP, NULL),
   
(24, 'Complaint Resolution Manager', 
   'Oversee the complaint resolution process and ensure customer satisfaction.', 
   'Experience in customer service management. Strong leadership and communication skills. Problem-solving mindset.', 
   'Competitive salary, health benefits, and professional growth opportunities.', 
   'Remote', 
   70000, 90000, 
   0, -- FullTime
   1, -- Intermediate
   CURRENT_DATE + INTERVAL '45 days', 
   true, 
   9, 
   CURRENT_TIMESTAMP, NULL);

-- Reset the sequence to continue after our manually inserted IDs
SELECT setval('public."jobs_Id_seq"', 24, true);





-- Insert JobCategory relationships
INSERT INTO public."job_categories" ("Id", "JobId", "CategoryId", "CreatedAt", "UpdatedAt")
VALUES
-- Information Technology jobs (Category 1)
(1, 1, 1, CURRENT_TIMESTAMP, NULL),  -- Senior Software Engineer at Google
(2, 3, 1, CURRENT_TIMESTAMP, NULL),  -- Data Scientist at Google
(3, 11, 1, CURRENT_TIMESTAMP, NULL), -- Game Developer at Autocompete
(4, 13, 1, CURRENT_TIMESTAMP, NULL), -- AI Research Scientist at Google DeepMind
(5, 14, 1, CURRENT_TIMESTAMP, NULL), -- Machine Learning Engineer at Google DeepMind
(6, 21, 1, CURRENT_TIMESTAMP, NULL), -- Natural Language Processing Engineer at Google AI

-- Business & Marketing jobs (Category 2)
(7, 2, 2, CURRENT_TIMESTAMP, NULL),  -- Product Manager at Google
(8, 9, 2, CURRENT_TIMESTAMP, NULL),  -- SEO Specialist at Google Map SEO Service
(9, 10, 2, CURRENT_TIMESTAMP, NULL), -- Local Marketing Consultant at Google Map SEO Service
(10, 17, 2, CURRENT_TIMESTAMP, NULL), -- Digital Marketing Specialist at Google Ads Mastermind
(11, 18, 2, CURRENT_TIMESTAMP, NULL), -- PPC Campaign Manager at Google Ads Mastermind
(12, 20, 2, CURRENT_TIMESTAMP, NULL), -- AI Product Manager at Google AI

-- Finance & Accounting jobs (Category 3)
-- No direct finance jobs in our dataset, but we can add some cross-category relationships
(13, 2, 3, CURRENT_TIMESTAMP, NULL),  -- Product Manager at Google (business strategy aspect)

-- Healthcare & Pharmaceuticals jobs (Category 4)
-- No direct healthcare jobs in our dataset

-- Education & Training jobs (Category 5)
(14, 5, 5, CURRENT_TIMESTAMP, NULL),  -- Technical Writer at Google
(15, 8, 5, CURRENT_TIMESTAMP, NULL),  -- Content Editor at Latest News
(16, 19, 5, CURRENT_TIMESTAMP, NULL), -- Google Ads Trainer at Google Ads Mastermind

-- Media & Entertainment jobs (Category 6)
(17, 7, 6, CURRENT_TIMESTAMP, NULL),  -- Technology Reporter at Latest News
(18, 8, 6, CURRENT_TIMESTAMP, NULL),  -- Content Editor at Latest News
(19, 11, 6, CURRENT_TIMESTAMP, NULL), -- Game Developer at Autocompete
(20, 12, 6, CURRENT_TIMESTAMP, NULL), -- UI/UX Designer for Gaming at Autocompete

-- Engineering & Manufacturing jobs (Category 7)
(21, 13, 7, CURRENT_TIMESTAMP, NULL), -- AI Research Scientist at Google DeepMind
(22, 14, 7, CURRENT_TIMESTAMP, NULL), -- Machine Learning Engineer at Google DeepMind
(23, 21, 7, CURRENT_TIMESTAMP, NULL), -- Natural Language Processing Engineer at Google AI

-- Hospitality & Tourism jobs (Category 8)
-- No direct hospitality jobs in our dataset

-- Legal Services jobs (Category 9)
(24, 22, 9, CURRENT_TIMESTAMP, NULL), -- AI Ethics Researcher at Google AI (legal/ethical aspects)

-- Human Resources jobs (Category 10)
(25, 23, 10, CURRENT_TIMESTAMP, NULL), -- Customer Support Specialist at googlecomplaint.com
(26, 24, 10, CURRENT_TIMESTAMP, NULL), -- Complaint Resolution Manager at googlecomplaint.com

-- Additional cross-category relationships
(27, 4, 1, CURRENT_TIMESTAMP, NULL),  -- UX Designer at Google (IT aspect)
(28, 4, 6, CURRENT_TIMESTAMP, NULL),  -- UX Designer at Google (Media aspect)
(29, 15, 5, CURRENT_TIMESTAMP, NULL), -- Research Intern - AI Ethics at Google DeepMind (Education aspect)
(30, 15, 9, CURRENT_TIMESTAMP, NULL), -- Research Intern - AI Ethics at Google DeepMind (Legal aspect)
(31, 16, 2, CURRENT_TIMESTAMP, NULL), -- Technical Program Manager - AI Research at Google DeepMind (Business aspect)
(32, 16, 7, CURRENT_TIMESTAMP, NULL), -- Technical Program Manager - AI Research at Google DeepMind (Engineering aspect)
(33, 6, 5, CURRENT_TIMESTAMP, NULL),  -- Corporate Communications Specialist at Google (Education/Training aspect)
(34, 6, 6, CURRENT_TIMESTAMP, NULL);  -- Corporate Communications Specialist at Google (Media aspect)

-- Reset the sequence to continue after our manually inserted IDs
-- SELECT setval('public."jobcategories_Id_seq"', 34, true);


-- Insert JobApplication records
INSERT INTO public."job_applications" (
    "Id", 
    "CoverLetter", 
    "Status", 
    "AppliedDate", 
    "JobId", 
    "ApplicantId", 
    "CreatedAt", 
    "UpdatedAt"
)
VALUES
-- User 1 applications
(1, 
 'I am excited to apply for the Senior Software Engineer position at Google. With over 5 years of experience in software development using Java and Python, I believe I would be a great fit for your team. I have a strong background in developing scalable web applications and am passionate about creating technology that improves people''s lives.',
 1, -- In Review
 CURRENT_DATE - INTERVAL '15 days',
 1, -- Senior Software Engineer at Google
 1, -- User 1
 CURRENT_TIMESTAMP,
 NULL),
 
(2, 
 'As a data enthusiast with strong analytical skills, I am applying for the Data Scientist position at Google. My experience with statistical analysis and machine learning models would allow me to contribute immediately to your team''s goals.',
 2, -- Accepted
 CURRENT_DATE - INTERVAL '30 days',
 3, -- Data Scientist at Google
 1, -- User 1
 CURRENT_TIMESTAMP,
 NULL),

-- User 2 applications
(3, 
 'I am writing to express my interest in the UX Designer position at Google. With my background in user-centered design and experience creating intuitive interfaces, I believe I can help enhance the user experience of Google''s products.',
 0, -- Pending
 CURRENT_DATE - INTERVAL '7 days',
 4, -- UX Designer at Google
 2, -- User 2
 CURRENT_TIMESTAMP,
 NULL),
 
(4, 
 'I am excited about the opportunity to join Google DeepMind as an AI Research Scientist. My PhD research in machine learning and published papers in top conferences align perfectly with the cutting-edge work being done at DeepMind.',
 3, -- Rejected
 CURRENT_DATE - INTERVAL '45 days',
 13, -- AI Research Scientist at Google DeepMind
 2, -- User 2
 CURRENT_TIMESTAMP,
 NULL),

-- User 3 applications
(5, 
 'With my experience in game development and proficiency in JavaScript, I am confident that I would be a valuable addition to the Autocompete team as a Game Developer. I have created several web-based games and am passionate about creating engaging user experiences.',
 1, -- In Review
 CURRENT_DATE - INTERVAL '10 days',
 11, -- Game Developer at Autocompete
 3, -- User 3
 CURRENT_TIMESTAMP,
 NULL),
 
(6, 
 'I am applying for the Machine Learning Engineer position at Google DeepMind. My experience implementing and scaling machine learning models for production environments makes me well-suited for this role.',
 0, -- Pending
 CURRENT_DATE - INTERVAL '3 days',
 14, -- Machine Learning Engineer at Google DeepMind
 3, -- User 3
 CURRENT_TIMESTAMP,
 NULL),

-- User 4 applications
(7, 
 'As a digital marketing professional with expertise in PPC advertising, I am excited to apply for the PPC Campaign Manager position at Google Ads Mastermind. I have managed successful Google Ads campaigns for various clients and am Google Ads certified.',
 2, -- Accepted
 CURRENT_DATE - INTERVAL '20 days',
 18, -- PPC Campaign Manager at Google Ads Mastermind
 4, -- User 4
 CURRENT_TIMESTAMP,
 NULL),
 
(8, 
 'I am interested in the AI Product Manager position at Google Artificial Intelligence. My background in product management combined with my understanding of AI technologies would allow me to effectively bridge the gap between technical development and business strategy.',
 1, -- In Review
 CURRENT_DATE - INTERVAL '12 days',
 20, -- AI Product Manager at Google AI
 4, -- User 4
 CURRENT_TIMESTAMP,
 NULL),

-- User 5 applications
(9, 
 'I am applying for the Technical Writer position at Google. With my strong writing skills and ability to explain complex technical concepts in clear, concise language, I believe I would excel in creating documentation for Google products.',
 0, -- Pending
 CURRENT_DATE - INTERVAL '5 days',
 5, -- Technical Writer at Google
 5, -- User 5
 CURRENT_TIMESTAMP,
 NULL),
 
(10, 
 'As someone passionate about AI ethics, I am excited to apply for the AI Ethics Researcher position at Google Artificial Intelligence. My background in both philosophy and computer science gives me a unique perspective on the ethical implications of AI technologies.',
 1, -- In Review
 CURRENT_DATE - INTERVAL '8 days',
 22, -- AI Ethics Researcher at Google AI
 5, -- User 5
 CURRENT_TIMESTAMP,
 NULL),

-- User 6 applications
(11, 
 'I am writing to express my interest in the Technology Reporter position at Latest News. With my background in technology journalism and strong writing skills, I am well-equipped to cover breaking news and developments in the technology sector.',
 2, -- Accepted
 CURRENT_DATE - INTERVAL '25 days',
 7, -- Technology Reporter at Latest News
 6, -- User 6
 CURRENT_TIMESTAMP,
 NULL),
 
(12, 
 'I am applying for the Content Editor position at Latest News. My experience in editing and content management, combined with my familiarity with technology topics, makes me a strong candidate for this role.',
 3, -- Rejected
 CURRENT_DATE - INTERVAL '40 days',
 8, -- Content Editor at Latest News
 6, -- User 6
 CURRENT_TIMESTAMP,
 NULL),

-- User 7 applications
(13, 
 'As an experienced SEO specialist with a focus on local SEO, I am excited to apply for the SEO Specialist position at Google Map SEO Service. I have helped numerous businesses improve their visibility on Google Maps and increase their local traffic.',
 1, -- In Review
 CURRENT_DATE - INTERVAL '14 days',
 9, -- SEO Specialist at Google Map SEO Service
 7, -- User 7
 CURRENT_TIMESTAMP,
 NULL),
 
(14, 
 'I am interested in the Local Marketing Consultant position at Google Map SEO Service. My experience in local marketing strategies and client management would allow me to effectively help businesses optimize their local online presence.',
 0, -- Pending
 CURRENT_DATE - INTERVAL '2 days',
 10, -- Local Marketing Consultant at Google Map SEO Service
 7, -- User 7
 CURRENT_TIMESTAMP,
 NULL),

-- User 8 applications
(15, 
 'I am applying for the Customer Support Specialist position at googlecomplaint.com. With my strong communication skills and experience in customer service, I am confident in my ability to handle customer complaints and inquiries effectively.',
 2, -- Accepted
 CURRENT_DATE - INTERVAL '18 days',
 23, -- Customer Support Specialist at googlecomplaint.com
 8, -- User 8
 CURRENT_TIMESTAMP,
 NULL),
 
(16, 
 'As someone with experience in customer service management, I am excited to apply for the Complaint Resolution Manager position at googlecomplaint.com. I have a proven track record of improving customer satisfaction and streamlining complaint resolution processes.',
 1, -- In Review
 CURRENT_DATE - INTERVAL '9 days',
 24, -- Complaint Resolution Manager at googlecomplaint.com
 8, -- User 8
 CURRENT_TIMESTAMP,
 NULL);

-- Reset the sequence to continue after our manually inserted IDs
SELECT setval('public."job_applications_Id_seq"', 16, true);


-- Insert SavedJob records
INSERT INTO public."saved_jobs" (
    "Id", 
    "SavedDate", 
    "JobId", 
    "UserId", 
    "CreatedAt", 
    "UpdatedAt"
)
VALUES
-- User 1 saved jobs
(1, 
 CURRENT_DATE - INTERVAL '10 days',
 4, -- UX Designer at Google
 1, -- User 1
 CURRENT_TIMESTAMP,
 NULL),
 
(2, 
 CURRENT_DATE - INTERVAL '8 days',
 13, -- AI Research Scientist at Google DeepMind
 1, -- User 1
 CURRENT_TIMESTAMP,
 NULL),

(3, 
 CURRENT_DATE - INTERVAL '5 days',
 20, -- AI Product Manager at Google AI
 1, -- User 1
 CURRENT_TIMESTAMP,
 NULL),

-- User 2 saved jobs
(4, 
 CURRENT_DATE - INTERVAL '15 days',
 1, -- Senior Software Engineer at Google
 2, -- User 2
 CURRENT_TIMESTAMP,
 NULL),
 
(5, 
 CURRENT_DATE - INTERVAL '12 days',
 14, -- Machine Learning Engineer at Google DeepMind
 2, -- User 2
 CURRENT_TIMESTAMP,
 NULL),

-- User 3 saved jobs
(6, 
 CURRENT_DATE - INTERVAL '20 days',
 7, -- Technology Reporter at Latest News
 3, -- User 3
 CURRENT_TIMESTAMP,
 NULL),
 
(7, 
 CURRENT_DATE - INTERVAL '18 days',
 17, -- Digital Marketing Specialist at Google Ads Mastermind
 3, -- User 3
 CURRENT_TIMESTAMP,
 NULL),

(8, 
 CURRENT_DATE - INTERVAL '7 days',
 22, -- AI Ethics Researcher at Google AI
 3, -- User 3
 CURRENT_TIMESTAMP,
 NULL),

-- User 4 saved jobs
(9, 
 CURRENT_DATE - INTERVAL '14 days',
 11, -- Game Developer at Autocompete
 4, -- User 4
 CURRENT_TIMESTAMP,
 NULL),
 
(10, 
 CURRENT_DATE - INTERVAL '9 days',
 21, -- Natural Language Processing Engineer at Google AI
 4, -- User 4
 CURRENT_TIMESTAMP,
 NULL),

-- User 5 saved jobs
(11, 
 CURRENT_DATE - INTERVAL '25 days',
 2, -- Product Manager at Google
 5, -- User 5
 CURRENT_TIMESTAMP,
 NULL),
 
(12, 
 CURRENT_DATE - INTERVAL '22 days',
 16, -- Technical Program Manager - AI Research at Google DeepMind
 5, -- User 5
 CURRENT_TIMESTAMP,
 NULL),

(13, 
 CURRENT_DATE - INTERVAL '6 days',
 24, -- Complaint Resolution Manager at googlecomplaint.com
 5, -- User 5
 CURRENT_TIMESTAMP,
 NULL),

-- User 6 saved jobs
(14, 
 CURRENT_DATE - INTERVAL '17 days',
 3, -- Data Scientist at Google
 6, -- User 6
 CURRENT_TIMESTAMP,
 NULL),
 
(15, 
 CURRENT_DATE - INTERVAL '11 days',
 9, -- SEO Specialist at Google Map SEO Service
 6, -- User 6
 CURRENT_TIMESTAMP,
 NULL),

-- User 7 saved jobs
(16, 
 CURRENT_DATE - INTERVAL '19 days',
 5, -- Technical Writer at Google
 7, -- User 7
 CURRENT_TIMESTAMP,
 NULL),
 
(17, 
 CURRENT_DATE - INTERVAL '13 days',
 15, -- Research Intern - AI Ethics at Google DeepMind
 7, -- User 7
 CURRENT_TIMESTAMP,
 NULL),

(18, 
 CURRENT_DATE - INTERVAL '4 days',
 19, -- Google Ads Trainer at Google Ads Mastermind
 7, -- User 7
 CURRENT_TIMESTAMP,
 NULL),

-- User 8 saved jobs
(19, 
 CURRENT_DATE - INTERVAL '16 days',
 6, -- Corporate Communications Specialist at Google
 8, -- User 8
 CURRENT_TIMESTAMP,
 NULL),
 
(20, 
 CURRENT_DATE - INTERVAL '3 days',
 12, -- UI/UX Designer for Gaming at Autocompete
 8, -- User 8
 CURRENT_TIMESTAMP,
 NULL);

-- Reset the sequence to continue after our manually inserted IDs
SELECT setval('public."saved_jobs_Id_seq"', 20, true);

SELECT setval('public."users_Id_seq"', 11, true);



