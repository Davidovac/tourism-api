CREATE TABLE Users (
   Id INTEGER PRIMARY KEY AUTOINCREMENT,
   Username TEXT,
   Password TEXT,
   Role TEXT
);

CREATE TABLE Tours (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT,
    Description TEXT,
    DateTime TEXT,
    MaxGuests INTEGER,
	Status TEXT,
	GuideId INTEGER,
    FOREIGN KEY (GuideId) REFERENCES Users(Id) ON DELETE CASCADE
);

CREATE TABLE KeyPoints (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
	OrderPosition INTEGER,
    Name TEXT,
    Description TEXT,
    ImageUrl TEXT,
    Latitude REAL,
	Longitude REAL
);

CREATE TABLE ToursKeypoints (
	Id INTEGER PRIMARY KEY AUTOINCREMENT,
	TourId INTEGER NOT NULL,
	KeyPointId INTEGER NOT NULL,
	FOREIGN KEY (TourId) REFERENCES Tours(Id) ON DELETE CASCADE,
	FOREIGN KEY (KeyPointId) REFERENCES KeyPoints(Id) ON DELETE CASCADE
);

CREATE TABLE Reservations (
	Id INTEGER PRIMARY KEY AUTOINCREMENT,
	GuestsCount INTEGER,
	UserId INTEGER NOT NULL,
	TourId INTEGER NOT NULL,
	FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
	FOREIGN KEY (TourId) REFERENCES Tours(Id) ON DELETE CASCADE
);

CREATE TABLE TourRatings (
	ID INTEGER PRIMARY KEY AUTOINCREMENT,
	TourId INTEGER NOT NULL,
	UserId INTEGER NOT NULL,
	Rating INTEGER,
	Comment TEXT,
	CreatedAt TEXT DEFAULT(DATE('now')),
	FOREIGN KEY (TourId) REFERENCES Tours(Id) ON DELETE CASCADE,
	FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);

CREATE TABLE Restaurants (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT,
    Description TEXT,
    Capacity INTEGER,
    ImageUrl TEXT,
    Latitude REAL,
	Longitude REAL,
	Status TEXT,
	OwnerId INTEGER,
	FOREIGN KEY (OwnerId) REFERENCES Users(Id) ON DELETE CASCADE
);

CREATE TABLE Meals (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
	OrderPosition INTEGER,
    Name TEXT,
    Price REAL,
    Ingredients TEXT,
    ImageUrl TEXT,
    RestaurantId INTEGER,
    FOREIGN KEY (RestaurantId) REFERENCES Restaurants(Id) ON DELETE CASCADE
);

CREATE TABLE RestaurantImages (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    RestaurantId INTEGER NOT NULL,
    ImageUrl TEXT NOT NULL,
    FOREIGN KEY (RestaurantId) REFERENCES Restaurants(Id) ON DELETE CASCADE
);

INSERT INTO Users (Id,Username,Password,Role) VALUES 
 (1,'turista1','turista1','turista'),
 (2,'turista2','turista2','turista'),
 (3,'turista3','turista3','turista'),
 (4,'turista4','turista4','turista'),
 (5,'turista5','turista5','turista'),
 (6,'turista6','turista6','turista'),
 (7,'vlasnik1','vlasnik1','vlasnik'),
 (8,'vlasnik2','vlasnik2','vlasnik'),
 (9,'vlasnik3','vlasnik3','vlasnik'),
 (10,'vodic1','vodic1','vodic'),
 (11,'vodic2','vodic2','vodic'),
 (12,'vodic3','vodic3','vodic');

INSERT INTO Tours (Id,Name,Description,DateTime,MaxGuests,Status,GuideId) VALUES 
 (1,'Beogradska Tura','Obilazak Beograda sa istorijskim lokalitetima','2025-04-01 10:00:00',20,'objavljeno',10),
 (2,'Novi Sad Obilazak','Istražite najbolje od Novog Sada','2025-04-02 09:00:00',15,'objavljeno',11),
 (3,'Niška Istorijska Tura','Šetnja kroz istoriju Niša','2025-04-03 11:00:00',10,'objavljeno',12),
 (4,'Zemunski Obala Tura','Scenski obilazak uz reku u Zemunu','2025-04-04 12:00:00',25,'objavljeno',10),
 (5,'Subotica Kulturna Tura','Istražite kulturnu baštinu Subotice','2025-04-05 08:00:00',12,'objavljeno',11);

INSERT INTO KeyPoints (Id,OrderPosition,Name,Description,ImageUrl,Latitude,Longitude) VALUES 
 (1,1,'Kalemegdan Tvrđava','Istorijska tvrđava u Beogradu sa prelepim pogledom','https://example.com/kalemegdan.jpg',44.8176,20.4633),
 (2,2,'Saborna Crkva','Lepa pravoslavna crkva u Novom Sadu','https://example.com/saborna.jpg',45.2671,19.8335),
 (3,3,'Muzej Nikole Tesle','Muzej posvećen Nikoli Tesli u Beogradu','https://example.com/tesla_museum.jpg',44.8228,20.4572),
 (4,1,'Petrovaradinska Tvrđava','Tvrđava sa prelepim pogledom na Novi Sad','https://example.com/petrovaradinska_tvrdjava.jpg',45.2461,19.8446),
 (5,2,'Zlatibor Priroda','Netaknuta priroda i staze na Zlatiboru','https://example.com/zlatibor_nature.jpg',43.7284,19.6089),
 (6,3,'Centar Novog Sada','Centar Novog Sada sa odličnim restoranima i prodavnicama','https://example.com/novi_sad_city.jpg',45.2671,19.8335),
 (7,1,'Niška Tvrđava','Tvrđava iz rimskog doba smeštena u Nišu','https://example.com/niska_tvrdjava.jpg',43.3147,21.8954),
 (8,2,'Crkva Svetog Save','Najveća crkva u Nišu','https://example.com/sveti_sava.jpg',43.3172,21.8961),
 (9,3,'Čele Kula','Spomenik žrtvama Prvog srpskog ustanka','https://example.com/cele_kula.jpg',43.3125,21.8958),
 (10,1,'Zemunska Obala','Uživajte u mirnom pogledu na reku sa Zemunske obale','https://example.com/zemun_quay.jpg',44.9862,20.4068),
 (11,2,'Gardos Toranj','Srednjovekovni toranj sa prelepim pogledom na reku','https://example.com/gardos_tower.jpg',44.9925,20.4031),
 (12,3,'Zemunska Crkva','Stara crkva sa bogatom istorijom','https://example.com/zemun_church.jpg',44.9867,20.406),
 (13,1,'Centar Subotice','Prelepa arhitektura i istorijske zgrade u centru Subotice','https://example.com/subotica_city.jpg',46.1003,19.6669),
 (14,2,'Palićko Jezero','Mirno jezero idealno za odmor','https://example.com/palic_lake.jpg',46.0705,19.7119),
 (15,3,'Subotička Sinagoga','Najveća sinagoga u Srbiji','https://example.com/subotica_synagogue.jpg',46.1003,19.6669);

 INSERT INTO ToursKeypoints (TourId, KeyPointId) VALUES
 (1,1),
 (1,2),
 (1,3),
 (2,4),
 (2,5),
 (2,6),
 (3,7),
 (3,8),
 (3,9),
 (4,10),
 (4,11),
 (4,12),
 (5,13),
 (5,14),
 (5,15);

INSERT INTO Restaurants (Id,Name,Description,Capacity,ImageUrl,Latitude,Longitude,Status,OwnerId) VALUES 
 (1,'Restoran Kalemegdan','Tradicionalna srpska hrana sa pogledom na grad',100,'https://example.com/kalemegdan_restaurant.jpg',44.8176,20.4633,'objavljeno',7),
 (2,'Restoran Novi Sad','Sveži lokalni sastojci i moderna srpska kuhinja',80,'https://example.com/novi_sad_restaurant.jpg',45.2671,19.8335,'objavljeno',8),
 (3,'Restoran Niš','Tradicionalna jela u srcu Niša',60,'https://example.com/nis_restaurant.jpg',43.3147,21.8954,'objavljeno',9),
 (4,'Restoran Zemun','Prijatna atmosfera sa odličnom srpskom hranom',50,'https://example.com/zemun_restaurant.jpg',44.9897,20.4067,'objavljeno',7),
 (5,'Restoran Subotica','Sveži lokalni sastojci sa modernim twistom',70,'https://example.com/subotica_restaurant.jpg',46.1003,19.6669,'objavljeno',8);

INSERT INTO Meals (Id,OrderPosition,Name,Price,Ingredients,ImageUrl,RestaurantId) VALUES 
 (1,1,'Pljeskavica',10.5,'Govedina, svinjetina, začini','https://example.com/pljeskavica.jpg',1),
 (2,2,'Ćevapi',8.5,'Mlevena govedina, jagnjetina, začini','https://example.com/cevapi.jpg',1),
 (3,1,'Ćevapi',7.5,'Mlevena govedina sa začinima','https://example.com/cevapi2.jpg',2),
 (4,2,'Sarma',10.0,'Kupus, svinjetina, mleveno meso','https://example.com/sarma2.jpg',2),
 (5,3,'Grilovana Riba',15.0,'Sveža riba sa povrćem','https://example.com/grilled_fish.jpg',2),
 (6,1,'Grilovana Riba',15.0,'Sveža riba sa povrćem','https://example.com/grilled_fish.jpg',3),
 (7,2,'Vegan Salata',7.0,'Lettuce, paradajz, krastavci, maslinovo ulje','https://example.com/vegan_salad.jpg',3),
 (8,1,'Zucchini Fritters',8.0,'Tikvice, brašno, začini','https://example.com/zucchini_fritters.jpg',4),
 (9,2,'Prženi Karp',14.5,'Sveže prženi šaran sa salatom','https://example.com/fried_carp.jpg',4),
 (10,3,'Burek',9.5,'Pita sa mesom ili sirom','https://example.com/burek.jpg',4),
 (11,1,'Pašticada',16.0,'Goveđi gulaš sa povrćem i vinom','https://example.com/pasticada.jpg',5),
 (12,2,'Pržena Piletina',10.5,'Hrskava pržena piletina sa pomfritom','https://example.com/fried_chicken.jpg',5),
 (13,3,'Gulaš',12.0,'Govedina, povrće, paprika','https://example.com/goulash.jpg',5);

 INSERT INTO RestaurantImages (RestaurantId, ImageUrl) VALUES
(1, 'https://example.com/kalemegdan_restaurant_1.jpg'),
(1, 'https://example.com/kalemegdan_restaurant_2.jpg'),
(2, 'https://example.com/novi_sad_restaurant_1.jpg'),
(2, 'https://example.com/novi_sad_restaurant_2.jpg'),
(3, 'https://example.com/nis_restaurant_1.jpg'),
(4, 'https://example.com/zemun_restaurant_1.jpg'),
(4, 'https://example.com/zemun_restaurant_2.jpg'),
(4, 'https://example.com/zemun_restaurant_3.jpg'),
(5, 'https://example.com/subotica_restaurant_1.jpg'),
(5, 'https://example.com/subotica_restaurant_2.jpg');