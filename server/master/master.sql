--*******************************************************************
--
-- The Multiverse Platform is made available under the MIT License.
--
-- Copyright (c) 2012 The Multiverse Foundation
-- 
-- Permission is hereby granted, free of charge, to any person 
-- obtaining a copy of this software and associated documentation 
-- files (the "Software"), to deal in the Software without restriction, 
-- including without limitation the rights to use, copy, modify, 
-- merge, publish, distribute, sublicense, and/or sell copies 
-- of the Software, and to permit persons to whom the Software 
-- is furnished to do so, subject to the following conditions:
-- 
-- The above copyright notice and this permission notice shall be 
-- included in all copies or substantial portions of the Software.
-- 
-- THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
-- EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES 
-- OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND 
-- NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
-- HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
-- WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
-- FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE 
-- OR OTHER DEALINGS IN THE SOFTWARE.
-- 
-- ********************************************************************

-- create a database called multiverse
-- create database multiverse;

-- switch to the multiverse database
use multiverse;

-- create a table for the masterserver - multiverse users (main account)
create table account (
    account_id INT NOT NULL AUTO_INCREMENT, 
    username VARCHAR(64) UNIQUE, 
    password VARCHAR(64), 
    email VARCHAR(64), 
    birthdate DATE,
    activated TINYINT(1),
    suspended TINYINT(1),
    activation_key VARCHAR(32),
    created_at DATETIME,
    last_modified_at DATETIME,
    INDEX (username), 
    PRIMARY KEY (account_id));

-- create a table of registered developers
create table developer (
    dev_id INT NOT NULL AUTO_INCREMENT, 
    email VARCHAR(64) NOT NULL UNIQUE,
    company VARCHAR(64), 
    password VARCHAR(64),
    size VARCHAR(64),
    skill VARCHAR(64),
    prior VARCHAR(64), 
    genre VARCHAR(64), 
    idea TEXT,
    INDEX (email), 
    PRIMARY KEY (dev_id));

-- to create a table of world servers
create table world (
    world_id INT NOT NULL AUTO_INCREMENT,
    dev_id INT NOT NULL,
    world_name VARCHAR(64) UNIQUE,
    pretty_name VARCHAR(64),
    description TEXT,
    server_name VARCHAR(64),
    server_port INT,
    public INT,
    approved TINYINT(1),
    patcher_URL VARCHAR(255),
    media_URL VARCHAR(255),
    logo_URL VARCHAR(255), 
    detail_URL VARCHAR(255),
    display_order INT,
    INDEX (world_name), 
    PRIMARY KEY (world_id));

-- create accounts
-- insert into account (username, password) values ("guest", "guestpasswd");

-- create some worlds: use full dns name for the server_name
insert into world (dev_id, world_name, server_name, server_port, patcher_URL, media_URL) 
    values (1, "sampleworld", "localhost", 5040, "http://MY_DOMAIN_NAME.COM/patcher/world_patcher.html", "http://update.MY_DOMAIN_NAME.COM/sampleworld/update/");
