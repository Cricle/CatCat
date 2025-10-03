-- Migration: Add location images to Pet table
-- Date: 2025-10-03
-- Description: Change location text fields to image + description fields

-- Add new image and description columns
ALTER TABLE Pets ADD COLUMN FoodLocationImage TEXT;
ALTER TABLE Pets ADD COLUMN FoodLocationDesc TEXT;
ALTER TABLE Pets ADD COLUMN WaterLocationImage TEXT;
ALTER TABLE Pets ADD COLUMN WaterLocationDesc TEXT;
ALTER TABLE Pets ADD COLUMN LitterBoxLocationImage TEXT;
ALTER TABLE Pets ADD COLUMN LitterBoxLocationDesc TEXT;
ALTER TABLE Pets ADD COLUMN CleaningSuppliesImage TEXT;
ALTER TABLE Pets ADD COLUMN CleaningSuppliesDesc TEXT;

-- Add missing columns from previous migration
ALTER TABLE Pets ADD COLUMN NeedsWaterRefill BOOLEAN DEFAULT false;
ALTER TABLE Pets ADD COLUMN SpecialInstructions TEXT;

-- Drop old location text columns (if they exist)
ALTER TABLE Pets DROP COLUMN IF EXISTS FoodLocation;
ALTER TABLE Pets DROP COLUMN IF EXISTS WaterLocation;
ALTER TABLE Pets DROP COLUMN IF EXISTS LitterBoxLocation;
ALTER TABLE Pets DROP COLUMN IF EXISTS CleaningSuppliesLocation;

