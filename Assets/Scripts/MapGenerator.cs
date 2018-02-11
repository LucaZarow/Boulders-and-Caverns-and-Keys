/*mostly taken from
https://unity3d.com/learn/tutorials/projects/procedural-cave-generation-tutorial
so i'll comment what i modified
*/


using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour {

	public int width;
	public int height;

	public string seed;
	public bool useRandomSeed;

	public BoulderManager boulderManager;
	public KeyManager keyManager;

	[Range(0,100)]
	public int randomFillPercent;

	int [,] map;
	int [,] unicursalMap;//grid of walls/empty spaces

	List<Coord> path;
	List<Coord> fullPath;//unicursal path along map
	List<Coord> keyPos;//position of keys
	Coord startCoord;
	Coord endCoord;

	int stuck = 0;//to help exit loops



	public void GenerateMap(){
		map = new int[width, height];
		unicursalMap = new int[width, height];
		path = new List<Coord> ();
		fullPath = new List<Coord> ();
		keyPos = new List<Coord> ();
		startCoord = new Coord ((int) (width*0.95), (int) (height*0.05));
		endCoord = new Coord ((int) (width*0.05), (int) (height*0.95));

		RandomFillMap ();
		BlankFillUnicursalMap ();

		for (int i = 0; i < 5; i ++) {
			SmoothMap();
		}

		ProcessMap ();

		int borderSize = 1;
		int[,] borderedMap =  new int[width + borderSize*2, height + borderSize*2];

		for (int i = 0; i < borderedMap.GetLength(0); i++) {
			for (int j = 0; j < borderedMap.GetLength(1); j++) {
				if (i > borderSize && i < width + borderSize && j >= borderSize && j< height + borderSize) {
					//borderedMap[i,j] = map[i-borderSize, j-borderSize];
					borderedMap[i,j] = unicursalMap[i-borderSize, j-borderSize];//now create mesh of unicursal map
				}else{
					borderedMap[i,j] = 1;
				}
			}
		}

		MeshGenerator meshGen = GetComponent<MeshGenerator> ();
		meshGen.GenerateMesh (borderedMap, 1);
	}
	//basically make a map where the rooms aren't rediculous
	void ProcessMap() {
		List<List<Coord>> wallRegions = GetRegions (1);
		int wallThresholdSize = (int) (height*width/300);

		foreach (List<Coord> wallRegion in wallRegions) {
			if (wallRegion.Count < wallThresholdSize) {
				foreach (Coord tile in wallRegion) {
					map [tile.tileX, tile.tileY] = 0;
				}
			}
		}

		List<List<Coord>> roomRegions = GetRegions (0);
		int roomThresholdSize = (int) (height*width/150);
		List<Room> survivingRooms = new List<Room>();

		foreach (List<Coord> roomRegion in roomRegions) {
			if (roomRegion.Count < roomThresholdSize) {
				foreach (Coord tile in roomRegion) {
					map [tile.tileX, tile.tileY] = 1;
				}
			} else {
				survivingRooms.Add (new Room (roomRegion, map));
			}
		}
		survivingRooms.Sort ();
		survivingRooms [0].isMainRoom = true;
		survivingRooms [0].isAccessibleFromMainRoom = true;
		ConnectClosestRooms (survivingRooms);
		CreateUnicursalPath ();//straight forward

		fullPath.Reverse ();//the order gets reversed because of the way Add words

		CreateAlcoves (3);//make 3 dead-ends

		SetBoulderPath ();//set up for boulder and keys
		SetKeyPosition ();
	}

	//pass key positions to key manager
	void SetKeyPosition(){
		List<Vector3> newPos = new List<Vector3> ();

		foreach (Coord point in keyPos) { 
			newPos.Add(new Vector3(-width/2 + 0.5f + point.tileX, -1.5f - 50, -height/2 + 0.5f + point.tileY));//5
		}
		keyManager.SetManagerPosition (newPos);
	}

	//pass boulder path to boulder manager
	void SetBoulderPath(){
		List<Vector3> newPath = new List<Vector3> ();

		foreach (Coord point in fullPath) { 
			newPath.Add(new Vector3(-width/2 + 0.5f + point.tileX, -1.5f - 50, -height/2 + 0.5f + point.tileY));
		}
		boulderManager.SetManagerPath (newPath);
	}

	void ConnectClosestRooms(List<Room> allRooms, bool forceAccessibilityFromMainRoom = false){

		List<Room> roomListA = new List<Room>();
		List<Room> roomListB = new List<Room>();

		if(forceAccessibilityFromMainRoom){
			foreach(Room room in allRooms){
				if(room.isAccessibleFromMainRoom){
					roomListB.Add(room);
				}else{
					roomListA.Add(room);
				}
			}
		}else{
			roomListA = allRooms;
			roomListB = allRooms;
		}

		int bestDistance = 0;
		Coord bestTileA = new Coord ();
		Coord bestTileB = new Coord ();
		Room bestRoomA = new Room ();
		Room bestRoomB = new Room ();
		bool possibleConnectionFound = false;

		foreach (Room roomA in roomListA) {
			if(!forceAccessibilityFromMainRoom){
				possibleConnectionFound = false;
				if(roomA.connectedRooms.Count > 0){
					continue;
				}
			}

			foreach (Room roomB in roomListB) {
				if(roomA == roomB || roomA.IsConnected(roomB)){
					continue;
				}

				for(int tileIndexA = 0; tileIndexA < roomA.edgeTiles.Count ; tileIndexA++){
					for(int tileIndexB = 0; tileIndexB < roomB.edgeTiles.Count ; tileIndexB++){
						Coord tileA = roomA.edgeTiles [tileIndexA];
						Coord tileB = roomB.edgeTiles [tileIndexB];
						int distanceBetweenRooms = (int) (Mathf.Pow ((tileA.tileX - tileB.tileX), 2) + Mathf.Pow ((tileA.tileY - tileB.tileY), 2)); 

						if (distanceBetweenRooms < bestDistance || !possibleConnectionFound) {
							bestDistance = distanceBetweenRooms;
							possibleConnectionFound = true;
							bestTileA = tileA;
							bestTileB = tileB;
							bestRoomA = roomA;
							bestRoomB = roomB;
						}
					}
				}
			}

			if (possibleConnectionFound && !forceAccessibilityFromMainRoom) {
				CreatePassage (bestRoomA, bestRoomB, bestTileA, bestTileB);
			}
		}

		if (possibleConnectionFound && forceAccessibilityFromMainRoom) {
			CreatePassage (bestRoomA, bestRoomB, bestTileA, bestTileB);
			ConnectClosestRooms (allRooms, true);
		}

		if (!forceAccessibilityFromMainRoom) {
			ConnectClosestRooms (allRooms, true);
		}
	}

	void CreatePassage(Room roomA, Room roomB, Coord tileA, Coord tileB){
		Room.ConnectRooms(roomA, roomB);

		if (path == null) {
			path.Add (tileA);
		}
		path.Add(tileB);

		List<Coord> line = GetLine (tileA, tileB);
		foreach (Coord c in line) {
			DrawCircle (c, 1, map);
		}
	}

	//in ConnectClosestRooms, the tutrial found the closest tiles to connect all rooms
	//i used those tiles as cornes to the path, assuming they'd be relatively sparse
	void CreateUnicursalPath(){
		Coord start;
		Coord end;
		List<Coord> line;

		path.Sort ();
		start = startCoord;

		for (int i = 0; i < path.Count; i++) {
			end = path [i];
			line = GetLine (start, end);

			foreach (Coord c in line) {
				fullPath.Add (c);
				DrawCircle (c, 1, unicursalMap);
			}
			start = end;
		}

		end = endCoord;
		line = GetLine (start, end);

		foreach (Coord c in line) {
			fullPath.Add (c);
			DrawCircle (c, 1, unicursalMap);
		}
	}

	//go along full path and find where to place alcove
	void CreateAlcoves(int numberOfAlcoves){
		int numberOfSections = numberOfAlcoves + 1;
		int position = 0;
		Coord start;
		Coord end;
		System.Random random = new System.Random ();

		for(int i = 0; i < numberOfAlcoves; i++) {

			position += fullPath.Count / numberOfSections;

			start = fullPath[position];
			end = CarveAlcove (start, random, 20);
			keyPos.Add (end);
		}
	}

	//carve out alcove and make sure that does not connect back to path
	Coord CarveAlcove(Coord start, System.Random random, int length){
		Coord end;
		List<Coord> line;

		do{
			do{
				end = RandomOnCircle (length, start, random);
			}while(!IsInMapRange(end.tileX, end.tileY));

			line = GetLine (start, end);

			stuck++;
			if(stuck>100){
				print("Stuck");
				break;
			}
		}while(IsIntersecting(line));

		stuck = 0;
		foreach (Coord c in line) {
			DrawCircle (c, 1, unicursalMap);
		}
		return line[line.Count - 2];
	}

	//find random point on circle
	Coord RandomOnCircle(int radius, Coord centre, System.Random random ){
		float angle = (float) (random.NextDouble () * Mathf.PI * 2.0);
		int x = (int)(Mathf.Cos (angle) * radius);
		int y = (int)(Mathf.Sin (angle) * radius);

		Coord point = new Coord (centre.tileX + x, centre.tileY + y);

		return point;
	}

	//check that zlcove is not intersecting full path
	bool IsIntersecting(List<Coord> line){

		int last = line.Count - 1;

		for (int x = line [last].tileX - 5; x < line [last].tileX + 5; x++) {
			for (int y = line [last].tileY - 5; y < line [last].tileY + 5; y++) {
				if(IsInMapRange(x,y)){
					if (unicursalMap [x, y] == 0) {
						return true;
					}
				}
			}
		}

		for (int i = line.Count - 1; i > line.Count/5; i--) {
			if (unicursalMap [line[i].tileX, line[i].tileY] == 0) {
				return true;
			}
		}
		return false;
	}

	void DrawCircle(Coord c, int r, int[,] map){
		for (int x = -r; x <= r; x++) {
			for (int y = -r; y <= r; y++) {
				if(x*x + y*y <= r*r){
					int drawX = c.tileX + x;
					int drawY = c.tileY + y;
					if(IsInMapRange(drawX, drawY)){
						map[drawX, drawY] = 0;
					}
				}
			}
		}
	}

	List<Coord> GetLine(Coord from, Coord to){
		List<Coord> line = new List<Coord> ();

		int x = from.tileX;
		int y = from.tileY;

		int dx = to.tileX - from.tileX;
		int dy = to.tileY - from.tileY;

		bool inverted = false;
		int step = Math.Sign (dx);
		int gradientStep = Math.Sign (dy);

		int longest = Math.Abs (dx);
		int shortest = Math.Abs (dy);

		if (longest < shortest) {
			inverted = true;
			longest =  Mathf.Abs (dy);
			shortest = Mathf.Abs (dx);

			step = Math.Sign (dy);
			gradientStep = Math.Sign (dx);
		}

		int gradientAccumulation = longest/2;
		for(int i = 0; i < longest; i++){
			line.Add(new Coord(x,y));

			if(inverted){
				y += step;
			}else{
				x += step;
			}

			gradientAccumulation += shortest;
			if(gradientAccumulation >= longest){
				if(inverted){
					x += gradientStep;
				}else{
					y += gradientStep;
				}
				gradientAccumulation -= longest;
			}
		}
		return line;
	}

	Vector3 CoordToWorldPoint(Coord tile){
		return new Vector3 (-width/2 + 0.5f + tile.tileX, 2, -height/2 + 0.5f + tile.tileY);
	}

	List<List<Coord>> GetRegions(int tileType){
		List<List<Coord>> regions = new List<List<Coord>> ();
		int[,] mapFlags = new int[width,height];

		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				if (mapFlags [x, y] == 0 & map [x, y] == tileType) {
					List<Coord> newRegion = GetRegionTiles (x, y);
					regions.Add (newRegion);
					foreach (Coord tile in newRegion) {
						mapFlags [tile.tileX, tile.tileY] = 1;
					}
				}
			}
		}

		return regions;
	}

	List<Coord> GetRegionTiles(int startX, int startY){
		List<Coord> tiles = new List<Coord> ();
		int[,] mapFlags = new int[width,height];
		int tileType = map [startX, startY];

		Queue<Coord> q = new Queue<Coord> ();
		q.Enqueue(new Coord(startX, startY));
		mapFlags [startX, startY] = 1;

		while (q.Count > 0) {
			Coord tile = q.Dequeue ();
			tiles.Add (tile);

			for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++) {
				for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++) {
					if(IsInMapRange(x,y) && (x==tile.tileX || y== tile.tileY)){
						if(mapFlags[x,y] == 0 & map[x,y] == tileType){
							mapFlags[x,y] = 1;
							q.Enqueue(new Coord(x,y));
						}
					}
				}
			}
		}

		return tiles;
	}

	bool IsInMapRange(int x, int y){
		return x >= 0 && x < width && y >= 0 && y < height;
	}

	void RandomFillMap(){
		if (useRandomSeed) {
			seed = Time.time.ToString();
		}

		System.Random pseudoRandom = new System.Random (seed.GetHashCode());

		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				if (x == 0 || x == width-1 || y == 0 || y == height -1) {
					map[x,y] = 1;
				} else {
					map[x,y] = (pseudoRandom.Next(0,100) < randomFillPercent)? 1: 0;
				}
			}
		}
	}
	//start with blank map and carve path later
	void BlankFillUnicursalMap(){
		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				unicursalMap[x,y] = 1;
			}
		}
	}

	void SmoothMap(){
		for (int i = 0; i < width; i++) {
			for (int j = 0; j < height; j++) {
				int neighbourWallTiles = GetSurroundingWallCount (i, j);

				if (neighbourWallTiles > 4) {
					map [i, j] = 1;
				} else if (neighbourWallTiles < 4) {
					map [i, j] = 0;
				}
			}
		}
	}

	int GetSurroundingWallCount(int gridX, int gridY){
		int wallCount = 0;
		for( int x = gridX-1; x <= gridX+1; x++){
			for( int y = gridY-1; y <= gridY+1; y++){
				if (IsInMapRange(x,y)) {
					if (x != gridX || y != gridY) {
						wallCount += map [x, y];
					}
				} else {
					wallCount++;
				}
			}
		}
		return wallCount;
	}

	class Coord : IComparable<Coord>{
		public int tileX;
		public int tileY;

		public Coord(){
		}

		public Coord(int x, int y){
			tileX = x;
			tileY = y;
		}

		public int CompareTo(Coord otherCoord){
			int otherX = otherCoord.tileX;
			int otherY = otherCoord.tileY;

			int distance = tileX  - tileY ;
			int otherDistance = otherX  - otherY ;

			return otherDistance.CompareTo (distance);//sorted from topleft to bottom right
		}
	}
		
	class Room : IComparable<Room>{
		public List<Coord> tiles;
		public List<Coord> edgeTiles;
		public List<Room> connectedRooms;
		public int roomSize;
		public bool isAccessibleFromMainRoom;
		public bool isMainRoom;

		public Room(){
		}

		public Room(List<Coord> roomTiles, int[,] map){
			tiles = roomTiles;
			roomSize = tiles.Count;
			connectedRooms = new List<Room>();
			edgeTiles = new List<Coord>();

			foreach(Coord tile in tiles){
				for(int x = tile.tileX - 1; x < tile.tileX + 1; x++){
					for(int y = tile.tileY - 1; y < tile.tileY + 1; y++){
						if(x == tile.tileX || y == tile.tileY){
							if(map[x,y] == 1){
								edgeTiles.Add(tile);
							}
						}
					}
				}
			}
		}

		public void SetAccessbileFromMainRoom(){
			if (!isAccessibleFromMainRoom) {
				isAccessibleFromMainRoom = true;
				foreach (Room connectedRooms in connectedRooms) {
					connectedRooms.SetAccessbileFromMainRoom ();
				}
			}
		}

		public static void ConnectRooms(Room roomA, Room roomB){
			if (roomA.isAccessibleFromMainRoom) {
				roomB.SetAccessbileFromMainRoom();
			}else if (roomB.isAccessibleFromMainRoom) {
				roomA.SetAccessbileFromMainRoom();
			}
			roomA.connectedRooms.Add (roomB);
			roomB.connectedRooms.Add (roomA);
		}

		public bool IsConnected(Room otherRoom){
			return connectedRooms.Contains (otherRoom);
		}

		public int CompareTo(Room otherRoom){
			return otherRoom.roomSize.CompareTo (roomSize);
		}
	}
}
