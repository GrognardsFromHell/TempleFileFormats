
#include <windows.h>
#include <stdio.h>
#include <string>
#include <vector>
#include <io.h>
#include <zlib.h>
#include <iostream>
#include <algorithm>

#define DEFAULT_COMPRESSION_LVL 9
int compressionLvl;
std::string pathToPack;

typedef void(__cdecl *TioMsgCb)(const char *msg);
typedef void(__cdecl *TioPackFuncs)(TioMsgCb errorCb, TioMsgCb msgCb);
typedef void(__cdecl *TioPack)(int argc, char *argv[]);
size_t fwrite_lk(void*buf, size_t size, size_t count, FILE*file);

enum TioArchiveFlags : uint32_t{
	TAF_UnhandledFile = 0,
	TAF_Uncompressed = 1,
	TAF_Compressed = 2,
	TAF_400 = 0x400,
};

struct TioArchive;

#pragma pack(push, 1)
struct tio_archive_entry{
	std::string name;
	TioArchiveFlags attr; // inited to 0x400
	int originalSize;
	int dataSize;
	int dataStart;
	int parent;
	int firstChild;
	int nextSibling;

	tio_archive_entry(){
		attr = TAF_UnhandledFile;
		originalSize = dataSize = dataStart = 0;
		parent = firstChild = nextSibling = -1;
	}
	
	tio_archive_entry(std::string & pathStr):tio_archive_entry(){
		name = pathStr;
	}

	bool IsAlreadyCompressed(FILE* file);
	bool AddToDatFile(FILE* datFile);
	BOOL WriteFileToFile(FILE*datFile, FILE*file); // copies file to datFile
	void ReportResultToConsole();
	bool GetParent(int i, TioArchive* tioArchive, int* parentId);
	bool GetSibling(int i, TioArchive* tioArchive, int* nextSibling);
	bool GetFirstChild(int i, TioArchive* tioArchive, int* childId);
	bool Serialize(FILE* datfile);
	
};
#pragma pop
struct TioArchive {
	std::string path;
	std::vector<tio_archive_entry> entries;
	void* lastOpenView;
	void* prevArchive;
	GUID archiveGuid;
	int field24;
	char* stringHeap;

	bool MakeArchive(FILE* datfile);
		

	void RemoveDriveFromPath();

	TioArchive(){
		lastOpenView = prevArchive = nullptr;
		field24 = 0;
		stringHeap = nullptr;
		memset(&this->archiveGuid,0, sizeof archiveGuid);
	}

	bool ParseArgs(int argc, char** argv);
	
protected:
	bool BuildFileList();
	void CreateFileEntries(std::string& pathStr);
	BOOL PackFiles(FILE*datFile);
	bool StoreFileInfo(FILE* datfile);
};
struct win_findstate{
	_finddata_t  finddata;
	int handle;
};

static void printError(const char *msg) {
	printf("[ERROR] %s", msg);
}

static void printInfo(const char *msg) {
	printf("[INFO] %s", msg);
}


int main(int argc, char * argv[]);

BOOL _tio_pack(int argC, char* argv[]);

BOOL PackFile(FILE* file, FILE* datFile);

int main(int argc, char* argv[])
{
	//auto lib = LoadLibrary("tio.dll");
	//auto lib = LoadLibrary("zlib.dll");

	//auto setFuncs = (TioPackFuncs) GetProcAddress(lib, "tio_pack_funcs");
	//auto tioPack = (TioPack) GetProcAddress(lib, "tio_pack");

	//setFuncs(printError, printInfo);

	TioArchive tioArchive;

	if (!tioArchive.ParseArgs(argc, argv)){
		return 1;
	}
	

	std::cout << "Creating database file " << tioArchive.path <<std::endl;
	auto datfile = fopen(tioArchive.path.c_str(), "wb");
	if (!datfile){
		std::cerr << "Error: Could not create output file " << tioArchive.path << std::endl;
		return 1;
	}

	compressionLvl = 9;
	std::cout << "Using compression level " << compressionLvl << "..." << std::endl;
	
	auto res = tioArchive.MakeArchive(datfile)?0:1;
	
	fclose(datfile);
	return res;

	
	/*tioPack(argc, argv);

	FreeLibrary(lib);
*/
	return 0;
}

bool tio_archive_entry::AddToDatFile(FILE * datFile){
	auto &entry = *this;
	std::cout << "Adding file " << entry.name << "..." ;
	std::string tmp(pathToPack);
	tmp.append(entry.name);
	auto file = fopen(tmp.c_str(), "rb");
	if (!file) {
		return false;
	}

	entry.dataStart = ftell(datFile);
	entry.originalSize = _filelength(_fileno(file));

	auto res = false;
	if (!entry.IsAlreadyCompressed(file)) {
		res = PackFile(file, datFile);
		if (res) {
			entry.attr = TAF_Compressed;
		}
	}
	if (!res) {
		res = WriteFileToFile(datFile, file);
		if (res) {
			entry.attr = TAF_Uncompressed;
		}
	}
	if (res)
		entry.dataSize = ftell(datFile) - entry.dataStart;

	fclose(file);
	return res;
}

BOOL TioArchive::PackFiles(FILE* datFile){
	auto pos0 = ftell(datFile);

	for (auto i=0; i < this->entries.size(); i++){
		auto &entry = this->entries[i];
		auto flags = entry.attr;

		auto curPos = ftell(datFile);
		if (curPos >= 0x38050){
			auto asdf = 1;
		}

		if ( !(flags & TAF_400)){
			if (!entry.AddToDatFile(datFile))
				return FALSE;
		}

		RemoveDriveFromPath();
		
		entry.ReportResultToConsole();
	}

	auto newPos = ftell(datFile);
	int archiveBodySize = newPos - pos0 + 4;
	fwrite(&archiveBodySize, 4, 1, datFile);
	return TRUE;
}

bool TioArchive::StoreFileInfo(FILE * datfile){
	auto pos0 = ftell(datfile);
	auto charCount = 0;
	auto entryCount = entries.size();
	if (fwrite_lk(&entryCount, sizeof(entryCount), 1, datfile) != 1)
		return false;

	for (auto i=0; i < entryCount; i++){
		auto &entry = this->entries[i];
		auto parentCount = 0;
		auto res = entry.GetParent(i, this, &entry.parent);
		if (res){
			std::cerr << "Error: no parent could be found for file " << entry.name << std::endl;
			return false;
		}

		res = entry.GetSibling(i, this, &entry.nextSibling);
		if (res){
			std::cerr << "Error: no sibling could be found for file " << entry.name << std::endl;
			return false;
		}

		auto childId = 0;
		res = entry.GetFirstChild(i, this, &entry.firstChild);
		if (res){
			std::cerr << "Error: no child could be found for file " << entry.name << std::endl;
			return false;
		}

		char fname[_MAX_FNAME], ext[_MAX_EXT];
		_splitpath(entry.name.c_str(), nullptr, nullptr, fname, ext);
		char newEntryName[_MAX_FNAME];
		_makepath(newEntryName, nullptr, nullptr, fname, ext);
		entry.name = newEntryName;


		auto nameSize = entry.name.size() + 1;
		charCount += nameSize;
		if (fwrite_lk(&nameSize, sizeof nameSize, 1, datfile) != 1
			|| fwrite_lk((void*)entry.name.c_str(), nameSize, 1, datfile) != 1
			|| !entry.Serialize(datfile))
			return 0;
	}

	// Write GUID to file end
	GUID fileGuid;
	char guidLabel[] = {"1TAD"};
	if (CoCreateGuid(&fileGuid)){
		sprintf(guidLabel, " TAD");
	}
	else{
		if (fwrite_lk(&fileGuid, sizeof GUID, 1, datfile) != 1)
			return false;
	}
	if (fwrite_lk(guidLabel, 4, 1, datfile) == 1 && fwrite_lk(&charCount, sizeof charCount, 1, datfile) == 1){
		auto newPos = ftell(datfile);
		auto bytesWritten = newPos - pos0 + 4;
		return fwrite_lk(&bytesWritten, sizeof bytesWritten, 1, datfile) == 1;
	}

	return false;
}

auto driveSkipper = [](std::string &path){
	auto pos = strchr(path.c_str(), ':');
	if (pos)
		pos = pos + 1;
	else
		pos = path.c_str();

	if (*pos == '\\') {
		if (pos[1] == '\\')
			pos = strchr(pos + 2, '\\');
		if (*pos == '\\') {
			pos++;
		}
	}
	if (*pos == '/')
		++pos;

	return pos;
};

auto TioArchiveEntryComparer = [](tio_archive_entry&a, tio_archive_entry&b) {
	auto aPos = driveSkipper(a.name);
	auto bPos = driveSkipper(b.name);
	return _stricmp(aPos, bPos) < 0;
};

bool TioArchive::BuildFileList(){


	auto lastChr =*( pathToPack.end() - 1);
	if (lastChr != '\\'){
		pathToPack.push_back('\\');
	}
	CreateFileEntries(std::string("")); // assumes recursion

	std::sort(this->entries.begin(), this->entries.end(), TioArchiveEntryComparer);

	// foregoing duplicates check...
	return true;
}


void TioArchive::CreateFileEntries(std::string & pathStr){
	// foregoing filename validity check
	auto fileIsValid = true;

	if (!fileIsValid)
		return;

	char _drive[10], _dir[_MAX_DIR], _fname[_MAX_FNAME], _ext[_MAX_EXT];
	

	_splitpath(pathToPack.c_str(), _drive, _dir, _fname, _ext);


	

	std::string tmp(_dir);
	if (pathStr.size()){
		tmp.append(pathStr);
	}
		

	/*while(tmp.size() > 1){
		tio_archive_entry newEntry;
		newEntry.attr = TAF_400;
		newEntry.name = tmp;
		entries.push_back(newEntry);
		tmp.pop_back();
		char tmp2[260];
		_splitpath(tmp.c_str(), nullptr, tmp2, nullptr, nullptr);
		tmp = tmp2;
	}*/
	char searchPath[_MAX_PATH];
	_makepath(searchPath, _drive, tmp.c_str(), "*", "*");

	win_findstate findData;
	findData.handle = _findfirst(searchPath, &findData.finddata);
	if (findData.handle == -1)
		return;

	
	char datFnameFull[_MAX_PATH];
	_fullpath(datFnameFull, this->path.c_str(), _MAX_PATH);

	do{
		if (findData.finddata.attrib & _A_SUBDIR){
			// assumes ok to do recursion...
			if (_stricmp(findData.finddata.name, ".") && _stricmp(findData.finddata.name, "..")){
				// valid folder for inclusion

				std::string subfolderPath(pathStr);
				subfolderPath.append(findData.finddata.name);
				tio_archive_entry newEntry(subfolderPath);
				newEntry.attr = TAF_400;
				entries.push_back(newEntry);

				subfolderPath.append("\\");
				CreateFileEntries(subfolderPath);

			}
		}
		else{
			
			std::string filePath(pathStr);
			filePath.append(findData.finddata.name);

			std::string tmp2(pathToPack);
			tmp2.append(filePath);
			char filePathFull[_MAX_PATH];
			_fullpath(filePathFull, tmp2.c_str(), _MAX_PATH);

			if (!_stricmp(datFnameFull, filePathFull)){
				continue;
			}

			tio_archive_entry newEntry;
			newEntry.name = filePath;
			newEntry.attr = TAF_UnhandledFile;
			entries.push_back(newEntry);
		}
	} while (_findnext(findData.handle, &findData.finddata) != -1);

	auto res = _findclose(findData.handle);
	memset(&findData.finddata, 0, sizeof findData.finddata);

}

bool TioArchive::MakeArchive(FILE* datfile){

	std::cout << "Building file list..." << std::endl;
	if (!BuildFileList()) {
		return false;
	}
	if (!PackFiles(datfile)) {
		return false;
	}

	if (!StoreFileInfo(datfile)) {
		return false;
	}

	auto curPos = ftell(datfile);
	std::cout << "Database " << this->path << " created: " << curPos << " total bytes." << std::endl;
	return true;
}

void TioArchive::RemoveDriveFromPath(){
	auto v11 = strchr(this->path.c_str(), ':');
	auto chrPtr = v11 ? v11 + 1 : this->path.c_str();
	if (*chrPtr == '\\') {
		if (chrPtr[1] == '\\')
			chrPtr = strchr(chrPtr + 2, '\\');
		if (*chrPtr == '\\')
			chrPtr++;
	}
	if (*chrPtr == '/')
		chrPtr++;
	if (chrPtr != this->path.c_str()) {
		this->path = chrPtr;
	}
}

bool TioArchive::ParseArgs(int argc, char ** argv){

	if (argc < 3) {
		std::cerr << "No database name or file provided!" << std::endl;
		return false;
	}

	for (auto i=1; i < argc; i++){
		auto arg = argv[i];
		if (*arg == '-' || *arg =='/'){
			auto swType = arg[1];
			switch(swType){
			case 'C':
			case 'c':
				compressionLvl = (arg[2] >= '0' && arg[2] <= '9')?arg[2]-'0':DEFAULT_COMPRESSION_LVL;
				break;
			default:
				break;
			}
			// flags processing and comprtession level... skipping for now
			continue;
		}

		this->path = arg;
		i++;
		if (i < argc)
			pathToPack = argv[i];
	}

	return this->path.size() != 0;
}

BOOL tio_archive_entry::WriteFileToFile(FILE * datFile, FILE * file){
	auto datfilePos = ftell(datFile);
	auto filePos = ftell(file);

	auto fileChr = fgetc(file);
	if (fileChr == -1)
		return TRUE;

	// copy file
	while (fputc(fileChr, datFile) != -1){
		fileChr = fgetc(file);
		if (fileChr == -1)
			return TRUE;
	}

	// error - cleanup and exit
	clearerr(datFile);
	clearerr(file);
	fseek(datFile, datfilePos, 0);
	fseek(file, filePos, 0);
	return FALSE;
}

void tio_archive_entry::ReportResultToConsole()
{
	auto &entry = *this;
	if (entry.attr == TAF_400) { // should this be & instead of == ? in the original it is ==
		std::cout << "Added directory " << entry.name << "..." << std::endl;
	}
	else {
		std::cout
			<< entry.originalSize << "->" << entry.dataSize
			<< "(" << (100.0 - 100.0*entry.dataSize / entry.originalSize) << "%%)"
			<< std::endl;
	}
}

void ParentGetter(TioArchive* tioArchive, tio_archive_entry&entry, std::string& buf){
	
	auto parId = entry.parent;
	if (parId != -1) {
		ParentGetter(tioArchive, tioArchive->entries[parId], buf);
		buf.append("\\");
	}

	buf.append(entry.name);
}

bool tio_archive_entry::GetParent(int idx, TioArchive * tioArchive, int * parentIdOut){

	char dir[_MAX_DIR];
	_splitpath(this->name.c_str(), nullptr, dir, nullptr, nullptr);
	if (parentIdOut)
		*parentIdOut = -1;

	if (!dir[0])
		return false;
	dir[strlen(dir) - 1] = 0; // remove trailing separator


	// search previous entries first (since it should be sorted)
	for (auto parId = idx-1; parId >= 0; parId--){
		auto &parentEntry = tioArchive->entries[parId];

		std::string parentPath;
		ParentGetter(tioArchive, parentEntry, parentPath);

		if (!_stricmp(parentPath.c_str(), dir)){
			if (parentIdOut)
				*parentIdOut = parId;
			return false;
		}
	}

	// if not found, search upwards
	for (auto parId = idx ; parId < tioArchive->entries.size(); parId++) {
		auto &parentEntry = tioArchive->entries[parId];

		std::string parentPath;
		ParentGetter(tioArchive, parentEntry, parentPath);
		if (!_stricmp(parentPath.c_str(), dir)) {
			if (parentIdOut)
				*parentIdOut = parId;
			return false;
		}
	}

	return true;
}

bool tio_archive_entry::GetSibling(int id, TioArchive* tioArchive, int* siblingIdOut){
	if (siblingIdOut)
		*siblingIdOut = -1;

	char dir[_MAX_DIR];
	_splitpath(this->name.c_str(), nullptr, dir, nullptr, nullptr);


	for (auto i = id + 1; i < tioArchive->entries.size(); i++) {

		char siblingDir[_MAX_DIR];
		_splitpath(tioArchive->entries[i].name.c_str(), nullptr, siblingDir, nullptr, nullptr);
		

		if (!_stricmp(siblingDir, dir)) {
			if (siblingIdOut)
				*siblingIdOut = i;
			return false;
		}

	}

	return false;
}

bool tio_archive_entry::GetFirstChild(int id, TioArchive * tioArchive, int * childIdOut){

	if (childIdOut)
		*childIdOut = -1;

	for (auto chId = id + 1; chId < tioArchive->entries.size(); chId++) {

		char dir[_MAX_DIR] = { 0, };
		_splitpath(tioArchive->entries[chId].name.c_str(), nullptr, dir, nullptr, nullptr);
		if (!dir[0]){
			continue;
		}
			
		dir[strlen(dir) - 1] = 0;
		if (!_stricmp(this->name.c_str(), dir)){
			if (childIdOut)
				*childIdOut = chId;
			return false;
		}

	}

	return false;
}

bool tio_archive_entry::Serialize(FILE * datfile){
	int ptrVal = (int)name.c_str();
	auto res = ( fwrite_lk(&ptrVal, sizeof (int), 1, datfile) == 1
			  && fwrite_lk((void*)&this->attr, sizeof(tio_archive_entry) - sizeof(std::string), 1, datfile) == 1);
	return res;
}

BOOL _tio_pack(int argC, char * argv[]){

	return 0;
}


size_t fwrite_lk(void*buf, size_t size, size_t count, FILE*file){
	_lock_file(file);
	auto res = fwrite(buf, size, count, file);
	_unlock_file(file);
	return res;
};
 
BOOL PackFile(FILE * file, FILE * datFile){

	auto datfilePos = ftell(datFile);
	auto filePos = ftell(file);

	z_stream zstr;
	zstr.zalloc = nullptr;
	zstr.zfree = nullptr;  
	zstr.opaque = nullptr;

	if (deflateInit_(&zstr, compressionLvl, "1.2.11", 56))
		return 0;

#define Z_BLOCK_SIZE 4096
	Bytef inputData[Z_BLOCK_SIZE];
	Bytef compressedData[Z_BLOCK_SIZE];

	zstr.avail_in = 0;
	zstr.next_in = inputData;
	zstr.next_out = compressedData;
	zstr.avail_out = Z_BLOCK_SIZE;

	auto fileNo = _fileno(file);
	auto fileLen = _filelength(fileNo);


	 auto cleanupAndExit = [file, datFile, datfilePos, filePos](z_streamp zstr){
		deflateEnd(zstr);
		clearerr(file);
		clearerr(datFile);
		fseek(datFile, datfilePos, 0);
		fseek(file, filePos, 0);
		return FALSE;
	};

	while (zstr.total_in != fileLen){
		if (zstr.avail_in == 0){
			auto readDataCount = fread(inputData, 1, Z_BLOCK_SIZE, file);
			zstr.next_in = inputData;
			zstr.avail_in = readDataCount;
		}
		if (!zstr.avail_out){

			 if (fwrite_lk(compressedData, Z_BLOCK_SIZE, 1, datFile) != 1){
				 return cleanupAndExit(&zstr);
			 }

			zstr.next_out = compressedData;
			zstr.avail_out = Z_BLOCK_SIZE;
		}

		if (deflate(&zstr, Z_NO_FLUSH)){
			return cleanupAndExit(&zstr);
		}
	}


	
	while (true){
		if (!zstr.avail_out){
			if (fwrite_lk(compressedData, Z_BLOCK_SIZE, 1, datFile) != 1){
				return cleanupAndExit(&zstr);
			}
			zstr.next_out = compressedData;
			zstr.avail_out = Z_BLOCK_SIZE;
		}
		auto deflateRes = deflate(&zstr, Z_FINISH);
		if (deflateRes == 1){
			if (zstr.avail_out != Z_BLOCK_SIZE)
				if ( fwrite_lk(compressedData, Z_BLOCK_SIZE- zstr.avail_out, 1, datFile) != 1){
					return cleanupAndExit(&zstr);
				}
			break;
		}
		if (deflateRes != 0)
			return cleanupAndExit(&zstr);
	}

	if (deflateEnd(&zstr) || fileLen <= ftell(datFile) - datfilePos){
		clearerr(file); clearerr(datFile);
		fseek(datFile, datfilePos, 0);
		fseek(file , filePos, 0);
		return FALSE;
	}

	return TRUE;
}

bool tio_archive_entry::IsAlreadyCompressed(FILE * file){
	char ext[100];
	_splitpath(this->name.c_str(), nullptr, nullptr, nullptr, ext);
	if (!_stricmp(ext, ".mp3") || !_stricmp(ext, ".bik"))
		return true;

	return false;
}
